import 'package:factory_queue_mobile/features/shipments/models/shipment_status.dart';
import 'package:factory_queue_mobile/features/shipments/providers/shipment_polling_provider.dart';
import 'package:factory_queue_mobile/features/shipments/screens/shipment_result_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class ShipmentStatusScreen extends ConsumerWidget {
  const ShipmentStatusScreen({required this.shipmentId, super.key});

  final String shipmentId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final status = ref.watch(shipmentPollingProvider(shipmentId));

    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(backgroundColor: Colors.transparent, title: const Text('Durum Takibi')),
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFFE0F2FE), Color(0xFFF5F7FB)],
          ),
        ),
        child: SafeArea(
          child: status.when(
            data: (value) => _content(context, value),
            error: (error, _) => _EmptyState(message: error.toString(), onRetry: () => ref.invalidate(shipmentPollingProvider(shipmentId))),
            loading: () => const _Skeleton(),
          ),
        ),
      ),
    );
  }

  Widget _content(BuildContext context, ShipmentStatus status) {
    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
      children: [
        _PremiumCard(
          gradient: LinearGradient(colors: [_statusColor(status.status), _statusColor(status.status).withValues(alpha: .72)]),
          child: Column(
            children: [
              Icon(_statusIcon(status.status), color: Colors.white, size: 54),
              const SizedBox(height: 12),
              const Text('Güncel Durum', style: TextStyle(color: Colors.white70, fontWeight: FontWeight.w700)),
              const SizedBox(height: 6),
              Text(status.displayStatusName, style: const TextStyle(color: Colors.white, fontSize: 28, fontWeight: FontWeight.w900)),
            ],
          ),
        ),
        const SizedBox(height: 18),
        _VerticalStepper(currentStatus: status.status),
        const SizedBox(height: 18),
        _PremiumCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Sıra Bilgileri', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
              const SizedBox(height: 14),
              _InfoTile(icon: Icons.confirmation_number_rounded, label: 'Sıra numarası', value: status.queueNumber?.toString() ?? '-'),
              _InfoTile(icon: Icons.directions_car_rounded, label: 'Önünüzdeki araç', value: status.vehiclesAhead?.toString() ?? '-'),
              _InfoTile(icon: Icons.radar_rounded, label: 'Tahmini durum', value: status.status == 6 ? 'Tamamlandı' : 'İşlem devam ediyor'),
            ],
          ),
        ),
        const SizedBox(height: 18),
        _PremiumCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Tartım Bilgileri', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
              const SizedBox(height: 14),
              _InfoTile(icon: Icons.scale_rounded, label: 'Dolu tartım', value: status.status >= 3 ? 'Kaydedildi' : 'Henüz yok'),
              _InfoTile(icon: Icons.scale_outlined, label: 'Boş tartım', value: status.status >= 6 ? 'Kaydedildi' : 'Henüz yok'),
              _InfoTile(icon: Icons.calculate_rounded, label: 'Net miktar', value: status.status >= 6 ? 'Sonuç ekranında' : 'Henüz yok'),
            ],
          ),
        ),
        const SizedBox(height: 20),
        FilledButton.icon(
          onPressed: status.status == 6 ? () => Navigator.of(context).push(_fadeRoute(ShipmentResultScreen(shipmentId: shipmentId))) : null,
          icon: const Icon(Icons.receipt_long_rounded),
          label: const Text('Sonucu Gör'),
        ),
      ],
    );
  }
}

class _VerticalStepper extends StatelessWidget {
  const _VerticalStepper({required this.currentStatus});

  final int currentStatus;

  @override
  Widget build(BuildContext context) {
    final steps = [
      ('Yolda', Icons.route_rounded, 0),
      ('Sırada', Icons.format_list_numbered_rounded, 1),
      ('Kantara Çağrıldı', Icons.campaign_rounded, 2),
      ('Kantarda', Icons.scale_rounded, 3),
      ('Boşaltımda', Icons.warehouse_rounded, 4),
      ('Tamamlandı', Icons.check_circle_rounded, 6),
    ];

    return _PremiumCard(
      child: Column(
        children: [
          for (final step in steps)
            _StepRow(
              title: step.$1,
              icon: step.$2,
              state: currentStatus > step.$3 ? _StepState.done : (currentStatus == step.$3 ? _StepState.current : _StepState.future),
            ),
        ],
      ),
    );
  }
}

enum _StepState { done, current, future }

class _StepRow extends StatelessWidget {
  const _StepRow({required this.title, required this.icon, required this.state});

  final String title;
  final IconData icon;
  final _StepState state;

  @override
  Widget build(BuildContext context) {
    final color = switch (state) {
      _StepState.done => const Color(0xFF22C55E),
      _StepState.current => const Color(0xFF0EA5E9),
      _StepState.future => Colors.grey.shade400,
    };
    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: Row(
        children: [
          CircleAvatar(backgroundColor: color.withValues(alpha: .15), child: Icon(icon, color: color)),
          const SizedBox(width: 14),
          Expanded(child: Text(title, style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700))),
          if (state == _StepState.done) const Icon(Icons.check_rounded, color: Color(0xFF22C55E)),
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
        boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: .06), blurRadius: 26, offset: const Offset(0, 16))],
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

class _Skeleton extends StatelessWidget {
  const _Skeleton();

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(20),
      children: const [
        _SkeletonBox(height: 160),
        SizedBox(height: 18),
        _SkeletonBox(height: 300),
        SizedBox(height: 18),
        _SkeletonBox(height: 180),
      ],
    );
  }
}

class _SkeletonBox extends StatelessWidget {
  const _SkeletonBox({required this.height});

  final double height;

  @override
  Widget build(BuildContext context) {
    return Container(height: height, decoration: BoxDecoration(color: Colors.white.withValues(alpha: .78), borderRadius: BorderRadius.circular(24)));
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState({required this.message, required this.onRetry});

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
              const Icon(Icons.sync_problem, size: 48, color: Color(0xFFEF4444)),
              const SizedBox(height: 14),
              Text('Durum bilgisi alınamadı', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
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

IconData _statusIcon(int status) => switch (status) {
      0 => Icons.route_rounded,
      1 => Icons.format_list_numbered_rounded,
      2 || 3 => Icons.scale_rounded,
      4 || 5 => Icons.warehouse_rounded,
      6 => Icons.check_circle_rounded,
      _ => Icons.info_rounded,
    };

Color _statusColor(int status) => switch (status) {
      0 => Colors.blueGrey,
      1 => const Color(0xFF0EA5E9),
      2 || 3 => const Color(0xFFF59E0B),
      4 || 5 => Colors.teal,
      6 => const Color(0xFF22C55E),
      _ => Colors.grey,
    };
