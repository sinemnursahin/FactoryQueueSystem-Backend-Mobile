import 'package:factory_queue_mobile/core/storage/secure_token_storage.dart';
import 'package:factory_queue_mobile/features/auth/models/auth_response.dart';
import 'package:factory_queue_mobile/features/auth/models/login_request.dart';
import 'package:factory_queue_mobile/features/auth/models/register_request.dart';
import 'package:factory_queue_mobile/features/auth/services/auth_service.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final authProvider = NotifierProvider<AuthNotifier, AuthState>(AuthNotifier.new);

class AuthState {
  const AuthState({this.user, this.isLoading = false, this.error});

  final AuthResponse? user;
  final bool isLoading;
  final String? error;

  bool get isAuthenticated => user != null;

  AuthState copyWith({AuthResponse? user, bool? isLoading, String? error, bool clearError = false}) {
    return AuthState(
      user: user ?? this.user,
      isLoading: isLoading ?? this.isLoading,
      error: clearError ? null : error ?? this.error,
    );
  }
}

class AuthNotifier extends Notifier<AuthState> {
  late final AuthService _authService;
  late final SecureTokenStorage _storage;

  @override
  AuthState build() {
    _authService = ref.watch(authServiceProvider);
    _storage = ref.watch(secureTokenStorageProvider);
    return const AuthState();
  }

  Future<void> login(String emailOrPhone, String password) async {
    await _authenticate(() => _authService.login(LoginRequest(emailOrPhone: emailOrPhone, password: password)));
  }

  Future<void> register({
    required String firstName,
    required String lastName,
    required String plateNumber,
    required String? email,
    required String? phoneNumber,
    required String password,
  }) async {
    await _authenticate(
      () => _authService.register(
        RegisterRequest(
          firstName: firstName,
          lastName: lastName,
          email: email,
          phoneNumber: phoneNumber,
          plateNumber: plateNumber,
          password: password,
        ),
      ),
    );
  }

  Future<void> logout() async {
    await _storage.clearToken();
    state = const AuthState();
  }

  Future<void> _authenticate(Future<AuthResponse> Function() action) async {
    state = state.copyWith(isLoading: true, clearError: true);
    try {
      final user = await action();
      await _storage.saveToken(user.token);
      state = AuthState(user: user);
    } catch (error) {
      state = AuthState(error: error.toString());
    }
  }
}
