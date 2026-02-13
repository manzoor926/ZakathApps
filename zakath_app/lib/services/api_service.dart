// lib/services/api_service.dart
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../utils/constants.dart';

const String _tokenKey = 'zakath_jwt_token';

/// Custom exception so we can surface the server's error message.
class ApiException implements Exception {
  final int statusCode;
  final String serverMessage;
  ApiException({required this.statusCode, required this.serverMessage});

  @override
  String toString() => serverMessage;
}

class ApiService {
  late final Dio dio;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  ApiService() {
    dio = Dio(BaseOptions(
      baseUrl: AppConstants.baseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 15),
      headers: {'Content-Type': 'application/json'},
    ));

    // Interceptor: inject Bearer token + translate errors
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _storage.read(key: _tokenKey);
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (error, handler) {
        if (error.response != null) {
          final body = error.response!.data;
          final msg = (body is Map && body.containsKey('message'))
              ? body['message'] as String
              : error.message ?? 'Unknown error';
          return handler.reject(DioException(
            requestOptions: error.requestOptions,
            response: error.response,
            error: ApiException(statusCode: error.response!.statusCode ?? 0, serverMessage: msg),
          ));
        }
        return handler.next(error);
      },
    ));
  }

  Future<void> saveToken(String token) => _storage.write(key: _tokenKey, value: token);
  Future<String?> getToken()           => _storage.read(key: _tokenKey);
  Future<void> clearToken()            => _storage.delete(key: _tokenKey);
}

// Global singleton — every service file imports this one instance
final ApiService apiService = ApiService();