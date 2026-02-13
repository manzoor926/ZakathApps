// lib/services/auth_service.dart
import '../models/models.dart';
import 'api_service.dart';

class AuthService {
  Future<Map<String, dynamic>> register(RegisterRequest req) async {
    final res = await apiService.dio.post('/Auth/register', data: req.toJson());
    return res.data as Map<String, dynamic>;
  }

  Future<LoginResponse> login(LoginRequest req) async {
    final res = await apiService.dio.post('/Auth/login', data: req.toJson());
    final response = LoginResponse.fromJson(res.data as Map<String, dynamic>);
    if (response.success && response.token != null) {
      await apiService.saveToken(response.token!);
    }
    return response;
  }

  Future<UserModel> getProfile() async {
    final res = await apiService.dio.get('/Auth/profile');
    final data = res.data as Map<String, dynamic>;
    return UserModel.fromJson(data['data'] as Map<String, dynamic>);
  }

  Future<void> logout() => apiService.clearToken();
}

final AuthService authService = AuthService();
