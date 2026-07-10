import 'package:factory_queue_mobile/features/profile/models/profile.dart';
import 'package:factory_queue_mobile/features/profile/models/profile_vehicle.dart';
import 'package:factory_queue_mobile/features/profile/services/profile_service.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final profileProvider = FutureProvider<Profile>((ref) async {
  return ref.watch(profileServiceProvider).getProfile();
});

final profileVehiclesProvider = FutureProvider<List<ProfileVehicle>>((ref) async {
  return ref.watch(profileServiceProvider).getVehicles();
});

final profileUpdateProvider = Provider<ProfileUpdateController>((ref) {
  return ProfileUpdateController(ref);
});

class ProfileUpdateController {
  ProfileUpdateController(this._ref);

  final Ref _ref;

  Future<void> update({
    required String firstName,
    required String lastName,
    required String? email,
    required String? phoneNumber,
  }) async {
    await _ref.read(profileServiceProvider).updateProfile(
          firstName: firstName,
          lastName: lastName,
          email: email,
          phoneNumber: phoneNumber,
        );
    _ref.invalidate(profileProvider);
  }

  Future<void> addVehicle(String plateNumber) async {
    await _ref.read(profileServiceProvider).addVehicle(plateNumber);
    _ref.invalidate(profileVehiclesProvider);
  }

  Future<void> updateVehicle(String id, String plateNumber) async {
    await _ref.read(profileServiceProvider).updateVehicle(id, plateNumber);
    _ref.invalidate(profileVehiclesProvider);
  }

  Future<void> deleteVehicle(String id) async {
    await _ref.read(profileServiceProvider).deleteVehicle(id);
    _ref.invalidate(profileVehiclesProvider);
  }
}
