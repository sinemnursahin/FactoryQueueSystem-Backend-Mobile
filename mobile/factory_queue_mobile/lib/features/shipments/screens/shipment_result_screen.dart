import 'package:factory_queue_mobile/features/shipments/models/shipment_result.dart';
import 'package:factory_queue_mobile/features/shipments/providers/shipment_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class ShipmentResultScreen extends ConsumerWidget {
  const ShipmentResultScreen({required this.shipmentId, super.key});

  final String shipmentId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final result = ref.watch(shipmentResultProvider(shipmentId));

    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(backgroundColor: Colors.transparent, title: const Text('Sonuç')),
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFFDCFCE7), Color(0xFFF5F7FB)],
          ),
        ),
        child: SafeArea(
          child: result.when(
            data: (value) => _content(context, ref, value),
            error: (error, _) => _EmptyState(message: error.toString(), onRetry: () => ref.invalidate(shipmentResultProvider(shipmentId))),
            loading: () => const _Skeleton(),
          ),
        ),
      ),
    );
  }

  Widget _content(BuildContext context, WidgetRef ref, ShipmentResult result) {
    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
      children: [
        _PremiumCard(
          gradient: const LinearGradient(colors: [Color(0xFF22C55E), Color(0xFF16A34A)]),
          child: Column(
            children: [
              const Icon(Icons.check_circle_rounded, color: Colors.white, size: 70),
              const SizedBox(height: 14),
              const Text('Sevkiyat Tamamlandı', style: TextStyle(color: Colors.white, fontSize: 28, fontWeight: FontWeight.w900)),
              const SizedBox(height: 6),
              Text('İşlem başarıyla tamamlandı.', style: TextStyle(color: Colors.white.withValues(alpha: .82))),
            ],
          ),
        ),
        const SizedBox(height: 18),
        _PremiumCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Net Miktar', style: Theme.of(context).textTheme.labelLarge?.copyWith(color: Colors.black54)),
              const SizedBox(height: 8),
              Text(result.netAmount.toStringAsFixed(2), style: Theme.of(context).textTheme.displaySmall?.copyWith(fontWeight: FontWeight.w900, color: const Color(0xFF22C55E))),
            ],
          ),
        ),
        const SizedBox(height: 18),
        Row(
          children: [
            Expanded(child: _MetricCard(title: 'Dolu Ağırlık', value: result.loadedWeight.toStringAsFixed(2), icon: Icons.scale_rounded)),
            const SizedBox(width: 14),
            Expanded(child: _MetricCard(title: 'Boş Ağırlık', value: result.emptyWeight.toStringAsFixed(2), icon: Icons.scale_outlined)),
          ],
        ),
        const SizedBox(height: 14),
        _MetricCard(title: 'Dolu Tartım Tarihi', value: _formatDateTime(result.loadedWeighDate), icon: Icons.login_rounded),
        const SizedBox(height: 14),
        _MetricCard(title: 'Boş Tartım Tarihi', value: _formatDateTime(result.emptyWeighDate), icon: Icons.logout_rounded),
        const SizedBox(height: 14),
        _MetricCard(title: 'Tamamlanma Tarihi', value: _formatDateTime(result.completedAt), icon: Icons.event_available_rounded),
        const SizedBox(height: 20),
        FilledButton.icon(
          onPressed: () => Navigator.of(context).popUntil((route) => route.isFirst),
          icon: const Icon(Icons.dashboard_rounded),
          label: const Text('Dashboard'),
        ),
        const SizedBox(height: 12),
        OutlinedButton.icon(
          onPressed: () async {
            try {
              await ref.read(queueShipmentProvider).exitFacility(shipmentId);
              if (!context.mounted) return;
              ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Çıkış yapıldı. Yeni ziyaret başlatıldı.')));
              Navigator.of(context).popUntil((route) => route.isFirst);
            } catch (error) {
              if (!context.mounted) return;
              ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error.toString()), backgroundColor: const Color(0xFFEF4444)));
            }
          },
          icon: const Icon(Icons.exit_to_app_rounded),
          label: const Text('Tesisten Çıkış Yaptım'),
        ),
      ],
    );
  }
}

class _MetricCard extends StatelessWidget {
  const _MetricCard({required this.title, required this.value, required this.icon});

  final String title;
  final String value;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return _PremiumCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          CircleAvatar(backgroundColor: const Color(0xFF0EA5E9).withValues(alpha: .12), child: Icon(icon, color: const Color(0xFF0EA5E9))),
          const SizedBox(height: 14),
          Text(title, style: Theme.of(context).textTheme.labelLarge?.copyWith(color: Colors.black54)),
          const SizedBox(height: 6),
          Text(value, style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w900)),
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

class _Skeleton extends StatelessWidget {
  const _Skeleton();

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(20),
      children: const [
        _SkeletonBox(height: 180),
        SizedBox(height: 18),
        _SkeletonBox(height: 120),
        SizedBox(height: 18),
        _SkeletonBox(height: 170),
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
              const Icon(Icons.receipt_long_outlined, size: 48, color: Color(0xFFF59E0B)),
              const SizedBox(height: 14),
              Text('Sonuç hazır değil', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
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

String _formatDateTime(DateTime value) {
  final local = value.toLocal();
  final day = local.day.toString().padLeft(2, '0');
  final month = local.month.toString().padLeft(2, '0');
  final year = local.year;
  final hour = local.hour.toString().padLeft(2, '0');
  final minute = local.minute.toString().padLeft(2, '0');
  return '$day.$month.$year $hour:$minute';
}
