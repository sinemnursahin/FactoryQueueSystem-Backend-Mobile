import 'package:dio/dio.dart';
import 'package:factory_queue_mobile/core/api/api_client.dart';
import 'package:factory_queue_mobile/core/api/api_exception.dart';
import 'package:factory_queue_mobile/features/profile/models/profile.dart';
import 'package:factory_queue_mobile/features/profile/models/profile_vehicle.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final profileServiceProvider = Provider<ProfileService>((ref) {
  return ProfileService(ref.watch(apiClientProvider));
});

class ProfileService {
  ProfileService(this._dio);

  final Dio _dio;

  Future<Profile> getProfile() async {
    try {
      final response = await _dio.get('/api/profile');
      return Profile.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<Profile> updateProfile({
    required String firstName,
    required String lastName,
    required String? email,
    required String? phoneNumber,
  }) async {
    try {
      final response = await _dio.put(
        '/api/profile',
        data: {
          'firstName': firstName,
          'lastName': lastName,
          'email': email,
          'phoneNumber': phoneNumber,
        },
      );
      return Profile.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<List<ProfileVehicle>> getVehicles() async {
    try {
      final response = await _dio.get('/api/profile/vehicles');
      final data = response.data as List<dynamic>;
      return data.map((item) => ProfileVehicle.fromJson(item as Map<String, dynamic>)).toList();
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<ProfileVehicle> addVehicle(String plateNumber) async {
    try {
      final response = await _dio.post('/api/profile/vehicles', data: {'plateNumber': plateNumber});
      return ProfileVehicle.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<ProfileVehicle> updateVehicle(String id, String plateNumber) async {
    try {
      final response = await _dio.put('/api/profile/vehicles/$id', data: {'plateNumber': plateNumber});
      return ProfileVehicle.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<void> deleteVehicle(String id) async {
    try {
      await _dio.delete('/api/profile/vehicles/$id');
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  String _message(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic> && data['message'] is String) {
      return data['message'] as String;
    }
    return 'Profil isteği başarısız.';
  }
}
