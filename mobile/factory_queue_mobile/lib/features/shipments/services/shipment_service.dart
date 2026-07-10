import 'package:dio/dio.dart';
import 'package:factory_queue_mobile/core/api/api_client.dart';
import 'package:factory_queue_mobile/core/api/api_exception.dart';
import 'package:factory_queue_mobile/features/shipments/models/active_shipment.dart';
import 'package:factory_queue_mobile/features/shipments/models/shipment_result.dart';
import 'package:factory_queue_mobile/features/shipments/models/shipment_status.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final shipmentServiceProvider = Provider<ShipmentService>((ref) {
  return ShipmentService(ref.watch(apiClientProvider));
});

class ShipmentService {
  ShipmentService(this._dio);

  final Dio _dio;

  Future<ActiveShipment> getActive() async {
    try {
      final response = await _dio.get('/api/shipments/active');
      return ActiveShipment.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<ShipmentStatus> queue(String shipmentId) async {
    try {
      final response = await _dio.post('/api/shipments/$shipmentId/queue');
      return ShipmentStatus.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<ActiveShipment> assignVehicle(String shipmentId, {required String plateNumber}) async {
    try {
      final response = await _dio.post(
        '/api/shipments/$shipmentId/assign-vehicle',
        data: {'plateNumber': plateNumber},
      );
      return ActiveShipment.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<ActiveShipment> exitFacility(String shipmentId) async {
    try {
      final response = await _dio.post('/api/shipments/$shipmentId/exit-facility');
      return ActiveShipment.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<ShipmentStatus> getStatus(String shipmentId) async {
    try {
      final response = await _dio.get('/api/shipments/$shipmentId/status');
      return ShipmentStatus.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  Future<ShipmentResult> getResult(String shipmentId) async {
    try {
      final response = await _dio.get('/api/shipments/$shipmentId/result');
      return ShipmentResult.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error));
    }
  }

  String _message(DioException error) {
    if (error.response?.statusCode == 404 && error.requestOptions.path.endsWith('/result')) {
      return 'Sonuç henüz hazır değil';
    }

    final data = error.response?.data;
    if (data is Map<String, dynamic> && data['message'] is String) {
      return data['message'] as String;
    }
    return 'Sevkiyat isteği başarısız.';
  }
}
