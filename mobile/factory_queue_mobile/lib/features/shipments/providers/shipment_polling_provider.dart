import 'dart:async';

import 'package:factory_queue_mobile/features/shipments/models/shipment_status.dart';
import 'package:factory_queue_mobile/features/shipments/services/shipment_service.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final shipmentPollingProvider = StreamProvider.family<ShipmentStatus, String>((ref, shipmentId) {
  final controller = StreamController<ShipmentStatus>();
  Timer? timer;

  Future<void> tick() async {
    try {
      final status = await ref.read(shipmentServiceProvider).getStatus(shipmentId);
      if (!controller.isClosed) {
        controller.add(status);
      }
      if (status.status == 6) {
        timer?.cancel();
      }
    } catch (error, stackTrace) {
      if (!controller.isClosed) {
        controller.addError(error, stackTrace);
      }
    }
  }

  tick();
  timer = Timer.periodic(const Duration(seconds: 4), (_) => tick());

  ref.onDispose(() {
    timer?.cancel();
    controller.close();
  });

  return controller.stream;
});
