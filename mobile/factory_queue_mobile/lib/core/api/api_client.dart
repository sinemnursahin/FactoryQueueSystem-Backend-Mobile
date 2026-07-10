import 'package:dio/dio.dart';
import 'package:factory_queue_mobile/core/storage/secure_token_storage.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

var _hasLoggedApiBaseUrl = false;

final apiClientProvider = Provider<Dio>((ref) {
  final storage = ref.watch(secureTokenStorageProvider);
  final baseUrl = _selectedApiBaseUrl();
  if (kDebugMode && !_hasLoggedApiBaseUrl) {
    _hasLoggedApiBaseUrl = true;
    debugPrint('API base URL: $baseUrl');
  }

  final dio = Dio(
    BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 10),
      headers: {'Content-Type': 'application/json'},
    ),
  );

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await storage.readToken();
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
    ),
  );

  return dio;
});

String _selectedApiBaseUrl() {
  const configuredBaseUrl = String.fromEnvironment('API_BASE_URL');
  return configuredBaseUrl.isNotEmpty ? configuredBaseUrl : _defaultApiBaseUrl();
}

String _defaultApiBaseUrl() {
  if (kIsWeb) {
    return 'http://localhost:5141';
  }

  return defaultTargetPlatform == TargetPlatform.android
      ? 'http://192.168.68.111:5141'
      : 'http://localhost:5141';
}
