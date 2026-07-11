import 'package:factory_queue_mobile/core/validation/contact_validation.dart';
import 'package:factory_queue_mobile/core/validation/plate_input_formatter.dart';
import 'package:factory_queue_mobile/features/profile/models/profile_vehicle.dart';
import 'package:factory_queue_mobile/features/profile/providers/profile_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class MyVehiclesScreen extends ConsumerStatefulWidget {
  const MyVehiclesScreen({super.key});

  @override
  ConsumerState<MyVehiclesScreen> createState() => _MyVehiclesScreenState();
}

class _MyVehiclesScreenState extends ConsumerState<MyVehiclesScreen> {
  final _plateController = TextEditingController();
  bool _isSaving = false;
  String? _plateError;

  @override
  void dispose() {
    _plateController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final vehicles = ref.watch(profileVehiclesProvider);

    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(backgroundColor: Colors.transparent, title: const Text('My Vehicles')),
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFFE0F2FE), Color(0xFFF5F7FB)],
          ),
        ),
        child: SafeArea(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
            children: [
              _PremiumCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Add Vehicle', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
                    const SizedBox(height: 14),
                    TextField(
                      controller: _plateController,
                      textCapitalization: TextCapitalization.characters,
                      inputFormatters: [const PlateInputFormatter(), LengthLimitingTextInputFormatter(10)],
                      decoration: InputDecoration(labelText: 'Plaka', helperText: 'Örn: 34ABC123', errorText: _plateError),
                    ),
                    const SizedBox(height: 14),
                    FilledButton.icon(
                      onPressed: _isSaving ? null : () => _saveNew(),
                      icon: const Icon(Icons.add_rounded),
                      label: const Text('Add'),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 18),
              vehicles.when(
                data: (items) => Column(children: [for (final item in items) _VehicleTile(vehicle: item)]),
                error: (error, _) => _PremiumCard(child: Text(error.toString())),
                loading: () => const Center(child: Padding(padding: EdgeInsets.all(24), child: CircularProgressIndicator())),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _saveNew() async {
    final plate = ContactValidation.normalizePlate(_plateController.text);
    if (plate == null || !ContactValidation.isValidPlate(plate)) {
      setState(() => _plateError = ContactValidation.invalidPlateMessage);
      _showError(ContactValidation.invalidPlateMessage);
      return;
    }

    setState(() => _isSaving = true);
    try {
      await ref.read(profileUpdateProvider).addVehicle(plate);
      _plateController.clear();
      _plateError = null;
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Vehicle added.')));
      }
    } catch (error) {
      if (mounted) {
        _showError(error.toString());
      }
    } finally {
      if (mounted) {
        setState(() => _isSaving = false);
      }
    }
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message), backgroundColor: const Color(0xFFEF4444)));
  }
}

class _VehicleTile extends ConsumerWidget {
  const _VehicleTile({required this.vehicle});

  final ProfileVehicle vehicle;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: _PremiumCard(
        child: Row(
          children: [
            const CircleAvatar(child: Icon(Icons.local_shipping_rounded)),
            const SizedBox(width: 12),
            Expanded(child: Text(vehicle.plateNumber, style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w800))),
            IconButton(
              tooltip: 'Edit',
              icon: const Icon(Icons.edit_rounded),
              onPressed: () => _edit(context, ref),
            ),
            IconButton(
              tooltip: 'Delete',
              icon: const Icon(Icons.delete_outline_rounded),
              onPressed: () => _delete(context, ref),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _edit(BuildContext context, WidgetRef ref) async {
    final controller = TextEditingController(text: vehicle.plateNumber);
    final plate = await showDialog<String>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Plakayı Düzenle'),
        content: TextField(
          controller: controller,
          textCapitalization: TextCapitalization.characters,
          inputFormatters: [const PlateInputFormatter(), LengthLimitingTextInputFormatter(10)],
          decoration: const InputDecoration(labelText: 'Plaka', helperText: 'Örn: 34ABC123'),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.of(context).pop(), child: const Text('Cancel')),
          FilledButton(onPressed: () => Navigator.of(context).pop(controller.text.trim()), child: const Text('Save')),
        ],
      ),
    );
    controller.dispose();
    final normalizedPlate = ContactValidation.normalizePlate(plate);
    if (normalizedPlate == null || !context.mounted) {
      return;
    }
    if (!ContactValidation.isValidPlate(normalizedPlate)) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text(ContactValidation.invalidPlateMessage), backgroundColor: Color(0xFFEF4444)));
      return;
    }

    try {
      await ref.read(profileUpdateProvider).updateVehicle(vehicle.id, normalizedPlate);
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Vehicle updated.')));
      }
    } catch (error) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error.toString()), backgroundColor: const Color(0xFFEF4444)));
      }
    }
  }

  Future<void> _delete(BuildContext context, WidgetRef ref) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete Vehicle'),
        content: Text(vehicle.plateNumber),
        actions: [
          TextButton(onPressed: () => Navigator.of(context).pop(false), child: const Text('Cancel')),
          FilledButton(onPressed: () => Navigator.of(context).pop(true), child: const Text('Delete')),
        ],
      ),
    );
    if (confirmed != true || !context.mounted) {
      return;
    }

    try {
      await ref.read(profileUpdateProvider).deleteVehicle(vehicle.id);
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Vehicle deleted.')));
      }
    } catch (error) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error.toString()), backgroundColor: const Color(0xFFEF4444)));
      }
    }
  }
}

class _PremiumCard extends StatelessWidget {
  const _PremiumCard({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: .06), blurRadius: 26, offset: const Offset(0, 16))],
      ),
      child: child,
    );
  }
}
