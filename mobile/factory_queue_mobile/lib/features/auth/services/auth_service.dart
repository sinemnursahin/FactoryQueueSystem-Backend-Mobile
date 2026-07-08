import 'package:dio/dio.dart';
import 'package:factory_queue_mobile/core/api/api_client.dart';
import 'package:factory_queue_mobile/core/api/api_exception.dart';
import 'package:factory_queue_mobile/features/auth/models/auth_response.dart';
import 'package:factory_queue_mobile/features/auth/models/login_request.dart';
import 'package:factory_queue_mobile/features/auth/models/register_request.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final authServiceProvider = Provider<AuthService>((ref) {
  return AuthService(ref.watch(apiClientProvider));
});

class AuthService {
  AuthService(this._dio);

  final Dio _dio;

  Future<AuthResponse> login(LoginRequest request) async {
    try {
      final response = await _dio.post('/api/auth/login', data: request.toJson());
      return AuthResponse.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<AuthResponse> register(RegisterRequest request) async {
    try {
      final response = await _dio.post('/api/auth/register', data: request.toJson());
      return AuthResponse.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  String _message(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic> && data['message'] is String) {
      return data['message'] as String;
    }
    final uri = error.requestOptions.uri;
    final status = error.response?.statusCode;
    final detail = error.message ?? error.type.name;
    return 'İstek başarısız: $uri${status == null ? '' : ' ($status)'}. $detail';
  }
}
