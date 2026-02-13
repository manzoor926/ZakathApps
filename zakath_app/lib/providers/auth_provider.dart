// lib/providers/auth_provider.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/models.dart';
import '../services/auth_service.dart';
import '../services/api_service.dart';

final authProvider = StateNotifierProvider<AuthNotifier, UserModel?>(
  (ref) => AuthNotifier(),
);

class AuthNotifier extends StateNotifier<UserModel?> {
  AuthNotifier() : super(null);

  /// On app start: if a saved token exists, load the profile
  Future<void> checkAuth() async {
    final token = await apiService.getToken();
    if (token != null) {
      try {
        state = await authService.getProfile();
      } catch (_) {
        await apiService.clearToken();
        state = null;
      }
    }
  }

  Future<LoginResponse> login(LoginRequest req) async {
    final response = await authService.login(req);
    if (response.success && response.user != null) state = response.user;
    return response;
  }

  Future<Map<String, dynamic>> register(RegisterRequest req) async {
    return await authService.register(req);
  }

  Future<void> logout() async {
    await authService.logout();
    state = null;
  }
}
