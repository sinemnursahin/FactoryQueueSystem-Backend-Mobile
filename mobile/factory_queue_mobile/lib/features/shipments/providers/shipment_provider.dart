import 'package:factory_queue_mobile/features/shipments/models/active_shipment.dart';
import 'package:factory_queue_mobile/features/shipments/models/shipment_result.dart';
import 'package:factory_queue_mobile/features/shipments/services/shipment_service.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final activeShipmentProvider = FutureProvider<ActiveShipment>((ref) async {
  return ref.watch(shipmentServiceProvider).getActive();
});

final shipmentResultProvider = FutureProvider.family<ShipmentResult, String>((ref, shipmentId) async {
  return ref.watch(shipmentServiceProvider).getResult(shipmentId);
});

final queueShipmentProvider = Provider<QueueShipmentController>((ref) {
  return QueueShipmentController(ref);
});

class QueueShipmentController {
  QueueShipmentController(this._ref);

  final Ref _ref;

  Future<void> queue(String shipmentId) async {
    await _ref.read(shipmentServiceProvider).queue(shipmentId);
    _ref.invalidate(activeShipmentProvider);
  }

  Future<void> assignVehicle(String shipmentId, {required String plateNumber}) async {
    await _ref.read(shipmentServiceProvider).assignVehicle(shipmentId, plateNumber: plateNumber);
    _ref.invalidate(activeShipmentProvider);
  }

  Future<void> exitFacility(String shipmentId) async {
    await _ref.read(shipmentServiceProvider).exitFacility(shipmentId);
    _ref.invalidate(activeShipmentProvider);
  }
}
