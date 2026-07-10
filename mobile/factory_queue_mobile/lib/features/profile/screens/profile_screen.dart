import 'package:factory_queue_mobile/core/validation/contact_validation.dart';
import 'package:factory_queue_mobile/core/validation/phone_input_formatter.dart';
import 'package:factory_queue_mobile/features/profile/models/profile.dart';
import 'package:factory_queue_mobile/features/profile/providers/profile_provider.dart';
import 'package:factory_queue_mobile/features/profile/screens/my_vehicles_screen.dart';
import 'package:factory_queue_mobile/features/shipments/providers/shipment_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class ProfileScreen extends ConsumerStatefulWidget {
  const ProfileScreen({super.key});

  @override
  ConsumerState<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends ConsumerState<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  bool _initialized = false;
  bool _isSaving = false;

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final profile = ref.watch(profileProvider);

    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(backgroundColor: Colors.transparent, title: const Text('Profil')),
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFFE0F2FE), Color(0xFFF5F7FB)],
          ),
        ),
        child: SafeArea(
          child: profile.when(
            data: _content,
            error: (error, _) => _EmptyState(message: error.toString(), onRetry: () => ref.invalidate(profileProvider)),
            loading: () => const Center(child: CircularProgressIndicator()),
          ),
        ),
      ),
    );
  }

  Widget _content(Profile profile) {
    if (!_initialized) {
      _firstNameController.text = profile.firstName;
      _lastNameController.text = profile.lastName;
      _emailController.text = profile.email ?? '';
      _phoneController.text = PhoneInputFormatter.format(ContactValidation.phoneInputValue(profile.phoneNumber));
      _initialized = true;
    }

    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
      children: [
        _PremiumCard(
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Sürücü Bilgileri', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w800)),
                const SizedBox(height: 18),
                TextFormField(controller: _firstNameController, decoration: const InputDecoration(labelText: 'Ad'), validator: _required),
                const SizedBox(height: 14),
                TextFormField(controller: _lastNameController, decoration: const InputDecoration(labelText: 'Soyad'), validator: _required),
                const SizedBox(height: 14),
                TextFormField(
                  controller: _emailController,
                  decoration: const InputDecoration(labelText: 'E-posta', helperText: 'Örn: ad@example.com'),
                  keyboardType: TextInputType.emailAddress,
                  validator: _emailValidator,
                ),
                const SizedBox(height: 14),
                TextFormField(
                  controller: _phoneController,
                  decoration: const InputDecoration(
                    labelText: 'Telefon',
                    prefixIcon: _PhonePrefix(),
                    prefixIconConstraints: BoxConstraints(minWidth: 76),
                    helperText: 'Örn: 532 123 45 67',
                  ),
                  keyboardType: TextInputType.phone,
                  inputFormatters: const [PhoneInputFormatter()],
                  validator: _phoneValidator,
                ),
                const SizedBox(height: 22),
                FilledButton.icon(
                  onPressed: _isSaving ? null : _save,
                  icon: _isSaving
                      ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                      : const Icon(Icons.save_rounded),
                  label: Text(_isSaving ? 'Kaydediliyor...' : 'Kaydet'),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 14),
        OutlinedButton.icon(
          onPressed: () => Navigator.of(context).push(_fadeRoute(const MyVehiclesScreen())),
          icon: const Icon(Icons.local_shipping_rounded),
          label: const Text('My Vehicles'),
        ),
      ],
    );
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final email = ContactValidation.normalizeEmail(_emailController.text);
    final phone = ContactValidation.normalizePhone(_phoneController.text);
    if (email == null && phone == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text(ContactValidation.missingContactMessage), backgroundColor: Color(0xFFEF4444)));
      return;
    }

    setState(() => _isSaving = true);
    try {
      await ref.read(profileUpdateProvider).update(
            firstName: _firstNameController.text.trim(),
            lastName: _lastNameController.text.trim(),
            email: email,
            phoneNumber: phone,
          );
      ref.invalidate(activeShipmentProvider);
      if (!mounted) return;
      _phoneController.text = PhoneInputFormatter.format(ContactValidation.phoneInputValue(phone));
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Profil güncellendi.')));
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error.toString()), backgroundColor: const Color(0xFFEF4444)));
      }
    } finally {
      if (mounted) {
        setState(() => _isSaving = false);
      }
    }
  }

  String? _required(String? value) => value == null || value.trim().isEmpty ? 'Zorunlu alan.' : null;

  String? _emailValidator(String? value) {
    final email = ContactValidation.normalizeEmail(value);
    return email != null && !ContactValidation.isValidEmail(email) ? ContactValidation.invalidEmailMessage : null;
  }

  String? _phoneValidator(String? value) {
    final phone = ContactValidation.normalizePhone(value);
    return phone != null && !ContactValidation.isValidPhone(phone) ? ContactValidation.invalidPhoneMessage : null;
  }
}

class _PhonePrefix extends StatelessWidget {
  const _PhonePrefix();

  @override
  Widget build(BuildContext context) {
    return const Padding(
      padding: EdgeInsets.only(left: 12, right: 8),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text('+90'),
          SizedBox(width: 8),
          SizedBox(height: 22, child: VerticalDivider(width: 1)),
        ],
      ),
    );
  }
}

PageRouteBuilder<void> _fadeRoute(Widget page) {
  return PageRouteBuilder<void>(
    pageBuilder: (_, animation, _) => FadeTransition(opacity: animation, child: page),
  );
}

class _PremiumCard extends StatelessWidget {
  const _PremiumCard({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: .06), blurRadius: 26, offset: const Offset(0, 16))],
      ),
      child: child,
    );
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
              const Icon(Icons.person_off_rounded, size: 48, color: Color(0xFFEF4444)),
              const SizedBox(height: 14),
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
