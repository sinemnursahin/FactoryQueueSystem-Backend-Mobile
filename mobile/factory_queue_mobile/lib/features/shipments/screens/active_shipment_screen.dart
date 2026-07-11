import 'package:factory_queue_mobile/core/validation/contact_validation.dart';
import 'package:factory_queue_mobile/core/validation/plate_input_formatter.dart';
import 'package:factory_queue_mobile/features/auth/providers/auth_provider.dart';
import 'package:factory_queue_mobile/features/profile/screens/profile_screen.dart';
import 'package:factory_queue_mobile/features/shipments/models/active_shipment.dart';
import 'package:factory_queue_mobile/features/shipments/providers/shipment_provider.dart';
import 'package:factory_queue_mobile/features/shipments/screens/shipment_result_screen.dart';
import 'package:factory_queue_mobile/features/shipments/screens/shipment_status_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class ActiveShipmentScreen extends ConsumerStatefulWidget {
  const ActiveShipmentScreen({super.key});

  @override
  ConsumerState<ActiveShipmentScreen> createState() => _ActiveShipmentScreenState();
}

class _ActiveShipmentScreenState extends ConsumerState<ActiveShipmentScreen> {
  bool _isQueueing = false;
  bool _isAssigningVehicle = false;
  bool _isExitingFacility = false;
  final _plateController = TextEditingController();
  String? _plateError;

  @override
  void dispose() {
    _plateController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final shipment = ref.watch(activeShipmentProvider);

    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        title: const Text('Dashboard'),
        actions: [
          IconButton(
            onPressed: () => Navigator.of(context).push(_fadeRoute(const ProfileScreen())),
            icon: const Icon(Icons.person_rounded),
            tooltip: 'Profil',
          ),
          IconButton(
            onPressed: () => ref.read(authProvider.notifier).logout(),
            icon: const Icon(Icons.logout_rounded),
            tooltip: 'Çıkış Yap',
          ),
        ],
      ),
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFFE0F2FE), Color(0xFFF5F7FB)],
          ),
        ),
        child: SafeArea(
          child: shipment.when(
            data: _content,
            error: (error, _) => _EmptyState(
              title: 'Dashboard yüklenemedi',
              message: error.toString(),
              onRetry: () => ref.invalidate(activeShipmentProvider),
            ),
            loading: () => const _Skeleton(),
          ),
        ),
      ),
    );
  }

  Widget _content(ActiveShipment shipment) {
    if (shipment.status == 6) {
      return _completedContent(shipment);
    }

    return RefreshIndicator(
      onRefresh: () async => ref.invalidate(activeShipmentProvider),
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
        children: [
          Hero(
            tag: 'shipment-${shipment.id}',
            child: _PremiumCard(
              gradient: const LinearGradient(colors: [Color(0xFF0EA5E9), Color(0xFF0284C7)]),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const CircleAvatar(
                        backgroundColor: Colors.white24,
                        child: Icon(Icons.local_shipping_rounded, color: Colors.white),
                      ),
                      const Spacer(),
                      _StatusChip(status: shipment.status, text: shipment.displayStatusName, onDark: true),
                    ],
                  ),
                  const SizedBox(height: 22),
                  Text(shipment.driverName ?? 'Araç seçilmedi', style: const TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.w800)),
                  const SizedBox(height: 6),
                  Text(shipment.plateNumber ?? 'Araç plakası girin', style: const TextStyle(color: Colors.white70, fontSize: 16, fontWeight: FontWeight.w600)),
                ],
              ),
            ),
          ),
          const SizedBox(height: 18),
          if (!shipment.hasVehicle) ...[
            _VehicleAssignmentCard(
              plateController: _plateController,
              plateError: _plateError,
              isAssigning: _isAssigningVehicle,
              onAssign: () => _assignVehicle(shipment.id),
            ),
            const SizedBox(height: 18),
          ] else ...[
            _PremiumCard(
              child: _InfoTile(icon: Icons.local_shipping_rounded, label: 'Plaka', value: shipment.plateNumber ?? '-'),
            ),
            const SizedBox(height: 18),
          ],
          _ProgressStepper(currentStatus: shipment.status),
          const SizedBox(height: 18),
          _PremiumCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Sevkiyat Bilgileri', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
                const SizedBox(height: 16),
                _InfoTile(icon: Icons.inventory_2_rounded, label: 'Malzeme', value: shipment.rawMaterialName ?? '-'),
                _InfoTile(icon: Icons.business_rounded, label: 'Tedarikçi', value: shipment.supplierName ?? '-'),
                _InfoTile(icon: Icons.confirmation_number_rounded, label: 'Sıra numarası', value: shipment.queueNumber?.toString() ?? '-'),
                _InfoTile(icon: Icons.format_list_numbered_rounded, label: 'Sıra pozisyonu', value: shipment.queueNumber == null ? '-' : '#${shipment.queueNumber}'),
                _InfoTile(icon: Icons.directions_car_filled_rounded, label: 'Önünüzdeki araç', value: shipment.vehiclesAhead?.toString() ?? '-'),
                _InfoTile(icon: Icons.login_rounded, label: 'Geliş saati', value: _formatDateTime(shipment.createdAt)),
                _InfoTile(icon: Icons.schedule_rounded, label: 'Sıra saati', value: shipment.queuedAt == null ? '-' : _formatDateTime(shipment.queuedAt!)),
              ],
            ),
          ),
          const SizedBox(height: 20),
          FilledButton.icon(
            onPressed: shipment.canQueue && shipment.hasVehicle && !_isQueueing ? () => _queue(shipment.id) : null,
            icon: _isQueueing
                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                : const Icon(Icons.place_rounded),
            label: Text(_isQueueing ? 'Sıra alınıyor...' : 'Tesise Geldim'),
          ),
          const SizedBox(height: 12),
          OutlinedButton.icon(
            onPressed: () => Navigator.of(context).push(_fadeRoute(ShipmentStatusScreen(shipmentId: shipment.id))),
            icon: const Icon(Icons.timeline_rounded),
            label: const Text('Durumu Takip Et'),
          ),
        ],
      ),
    );
  }

  Widget _completedContent(ActiveShipment shipment) {
    return RefreshIndicator(
      onRefresh: () async => ref.invalidate(activeShipmentProvider),
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
        children: [
          _PremiumCard(
            gradient: const LinearGradient(colors: [Color(0xFF22C55E), Color(0xFF16A34A)]),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    const CircleAvatar(
                      backgroundColor: Colors.white24,
                      child: Icon(Icons.check_circle_rounded, color: Colors.white),
                    ),
                    const Spacer(),
                    _StatusChip(status: shipment.status, text: shipment.displayStatusName, onDark: true),
                  ],
                ),
                const SizedBox(height: 22),
                const Text('Sevkiyat tamamlandı', style: TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.w800)),
                const SizedBox(height: 6),
                Text(shipment.plateNumber ?? 'Araç seçilmedi', style: const TextStyle(color: Colors.white70, fontSize: 16, fontWeight: FontWeight.w600)),
              ],
            ),
          ),
          const SizedBox(height: 18),
          _PremiumCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Sonraki işlem', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
                const SizedBox(height: 10),
                const Text('Sonucu görüntüleyebilir veya tesisten çıkış yaparak yeni ziyaret başlatabilirsiniz.'),
              ],
            ),
          ),
          const SizedBox(height: 20),
          FilledButton.icon(
            onPressed: () => Navigator.of(context).push(_fadeRoute(ShipmentResultScreen(shipmentId: shipment.id))),
            icon: const Icon(Icons.receipt_long_rounded),
            label: const Text('Sonuç Ekranı'),
          ),
          const SizedBox(height: 12),
          OutlinedButton.icon(
            onPressed: _isExitingFacility ? null : () => _exitFacility(shipment.id),
            icon: _isExitingFacility
                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                : const Icon(Icons.exit_to_app_rounded),
            label: Text(_isExitingFacility ? 'Çıkış yapılıyor...' : 'Tesisten Çıkış Yaptım'),
          ),
        ],
      ),
    );
  }

  Future<void> _queue(String shipmentId) async {
    setState(() => _isQueueing = true);
    try {
      await ref.read(queueShipmentProvider).queue(shipmentId);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Sıra numarası oluşturuldu.')));
      Navigator.of(context).push(_fadeRoute(ShipmentStatusScreen(shipmentId: shipmentId)));
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error.toString()), backgroundColor: const Color(0xFFEF4444)));
      }
    } finally {
      if (mounted) {
        setState(() => _isQueueing = false);
      }
    }
  }

  Future<void> _assignVehicle(String shipmentId) async {
    final manualPlate = ContactValidation.normalizePlate(_plateController.text);
    if (manualPlate == null || !ContactValidation.isValidPlate(manualPlate)) {
      setState(() => _plateError = ContactValidation.invalidPlateMessage);
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text(ContactValidation.invalidPlateMessage), backgroundColor: Color(0xFFEF4444)));
      return;
    }

    setState(() => _isAssigningVehicle = true);
    try {
      await ref.read(queueShipmentProvider).assignVehicle(
            shipmentId,
            plateNumber: manualPlate,
          );
      _plateController.clear();
      _plateError = null;
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Plaka kaydedildi.')));
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error.toString()), backgroundColor: const Color(0xFFEF4444)));
      }
    } finally {
      if (mounted) {
        setState(() => _isAssigningVehicle = false);
      }
    }
  }

  Future<void> _exitFacility(String shipmentId) async {
    setState(() => _isExitingFacility = true);
    try {
      await ref.read(queueShipmentProvider).exitFacility(shipmentId);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Çıkış yapıldı. Yeni ziyaret başlatıldı.')));
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error.toString()), backgroundColor: const Color(0xFFEF4444)));
      }
    } finally {
      if (mounted) {
        setState(() => _isExitingFacility = false);
      }
    }
  }
}

class _VehicleAssignmentCard extends StatelessWidget {
  const _VehicleAssignmentCard({
    required this.plateController,
    required this.plateError,
    required this.isAssigning,
    required this.onAssign,
  });

  final TextEditingController plateController;
  final String? plateError;
  final bool isAssigning;
  final VoidCallback onAssign;

  @override
  Widget build(BuildContext context) {
    return _PremiumCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Araç', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
          const SizedBox(height: 16),
          TextField(
            controller: plateController,
            enabled: !isAssigning,
            textCapitalization: TextCapitalization.characters,
            inputFormatters: [const PlateInputFormatter(), LengthLimitingTextInputFormatter(10)],
            decoration: InputDecoration(labelText: 'Araç Plakası', helperText: 'Örn: 34ABC123', errorText: plateError),
          ),
          const SizedBox(height: 16),
          FilledButton.icon(
            onPressed: isAssigning ? null : onAssign,
            icon: isAssigning
                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                : const Icon(Icons.check_rounded),
            label: Text(isAssigning ? 'Kaydediliyor...' : 'Plakayı Kaydet'),
          ),
        ],
      ),
    );
  }
}

class _PremiumCard extends StatelessWidget {
  const _PremiumCard({required this.child, this.gradient});

  final Widget child;
  final Gradient? gradient;

  @override
  Widget build(BuildContext context) {
    return AnimatedContainer(
      duration: const Duration(milliseconds: 240),
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: gradient == null ? Colors.white : null,
        gradient: gradient,
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(color: Colors.black.withValues(alpha: .06), blurRadius: 26, offset: const Offset(0, 16)),
        ],
      ),
      child: child,
    );
  }
}

class _InfoTile extends StatelessWidget {
  const _InfoTile({required this.icon, required this.label, required this.value});

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: Row(
        children: [
          CircleAvatar(radius: 18, backgroundColor: const Color(0xFF0EA5E9).withValues(alpha: .10), child: Icon(icon, color: const Color(0xFF0EA5E9), size: 20)),
          const SizedBox(width: 12),
          Expanded(child: Text(label, style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.black54))),
          Text(value, style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w800)),
        ],
      ),
    );
  }
}

class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.status, required this.text, this.onDark = false});

  final int status;
  final String text;
  final bool onDark;

  @override
  Widget build(BuildContext context) {
    final color = _statusColor(status);
    return Chip(
      label: Text(text),
      side: BorderSide.none,
      backgroundColor: onDark ? Colors.white.withValues(alpha: .20) : color.withValues(alpha: .14),
      labelStyle: TextStyle(color: onDark ? Colors.white : color, fontWeight: FontWeight.w800),
    );
  }
}

class _ProgressStepper extends StatelessWidget {
  const _ProgressStepper({required this.currentStatus});

  final int currentStatus;

  @override
  Widget build(BuildContext context) {
    final steps = ['Yolda', 'Sırada', 'Kantar', 'Boşaltım', 'Tamam'];
    final activeIndex = switch (currentStatus) {
      <= 0 => 0,
      1 => 1,
      2 || 3 => 2,
      4 || 5 => 3,
      _ => 4,
    };

    return _PremiumCard(
      child: Row(
        children: [
          for (var i = 0; i < steps.length; i++) ...[
            Expanded(
              child: Column(
                children: [
                  AnimatedContainer(
                    duration: const Duration(milliseconds: 260),
                    width: 30,
                    height: 30,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: i < activeIndex ? const Color(0xFF22C55E) : (i == activeIndex ? const Color(0xFF0EA5E9) : Colors.grey.shade300),
                    ),
                    child: Icon(i < activeIndex ? Icons.check_rounded : Icons.circle, size: 16, color: Colors.white),
                  ),
                  const SizedBox(height: 8),
                  Text(steps[i], textAlign: TextAlign.center, style: Theme.of(context).textTheme.labelSmall),
                ],
              ),
            ),
            if (i != steps.length - 1)
              Expanded(child: Divider(thickness: 2, color: i < activeIndex ? const Color(0xFF22C55E) : Colors.grey.shade300)),
          ],
        ],
      ),
    );
  }
}

class _Skeleton extends StatelessWidget {
  const _Skeleton();

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(20),
      children: const [
        _SkeletonBox(height: 150),
        SizedBox(height: 18),
        _SkeletonBox(height: 110),
        SizedBox(height: 18),
        _SkeletonBox(height: 260),
      ],
    );
  }
}

class _SkeletonBox extends StatelessWidget {
  const _SkeletonBox({required this.height});

  final double height;

  @override
  Widget build(BuildContext context) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: .35, end: .9),
      duration: const Duration(milliseconds: 850),
      builder: (_, value, _) => Container(
        height: height,
        decoration: BoxDecoration(color: Colors.white.withValues(alpha: value), borderRadius: BorderRadius.circular(24)),
      ),
      onEnd: () {},
    );
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState({required this.title, required this.message, required this.onRetry});

  final String title;
  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: _PremiumCard(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.local_shipping_outlined, size: 48, color: Color(0xFF0EA5E9)),
              const SizedBox(height: 14),
              Text(title, style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
              const SizedBox(height: 8),
              Text(message, textAlign: TextAlign.center),
              const SizedBox(height: 18),
              FilledButton(onPressed: onRetry, child: const Text('Tekrar Dene')),
            ],
          ),
        ),
      ),
    );
  }
}

PageRouteBuilder<void> _fadeRoute(Widget page) {
  return PageRouteBuilder<void>(
    pageBuilder: (_, animation, _) => FadeTransition(opacity: animation, child: page),
  );
}

Color _statusColor(int status) => switch (status) {
      0 => Colors.blueGrey,
      1 => const Color(0xFF0EA5E9),
      2 || 3 => const Color(0xFFF59E0B),
      4 || 5 => Colors.teal,
      6 => const Color(0xFF22C55E),
      _ => Colors.grey,
    };

String _formatDateTime(DateTime value) {
  final local = value.toLocal();
  final day = local.day.toString().padLeft(2, '0');
  final month = local.month.toString().padLeft(2, '0');
  final year = local.year;
  final hour = local.hour.toString().padLeft(2, '0');
  final minute = local.minute.toString().padLeft(2, '0');
  return '$day.$month.$year $hour:$minute';
}
