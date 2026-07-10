import 'dart:ui';

import 'package:factory_queue_mobile/core/validation/contact_validation.dart';
import 'package:factory_queue_mobile/core/validation/phone_input_formatter.dart';
import 'package:factory_queue_mobile/features/auth/providers/auth_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;
  String? _emailError;
  String? _phoneError;

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(authProvider, (previous, next) {
      if (previous?.isLoading == true && next.error != null) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(next.error!), backgroundColor: const Color(0xFFEF4444)));
      }
      if (previous?.isLoading == true && next.isAuthenticated) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Kayıt oluşturuldu.')));
      }
    });

    final auth = ref.watch(authProvider);

    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [Color(0xFFE0F2FE), Color(0xFFF5F7FB), Color(0xFFFEF3C7)],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 520),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(24),
                  child: BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
                    child: Container(
                      padding: const EdgeInsets.all(24),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: .82),
                        borderRadius: BorderRadius.circular(24),
                        border: Border.all(color: Colors.white.withValues(alpha: .7)),
                        boxShadow: [
                          BoxShadow(color: const Color(0xFF0EA5E9).withValues(alpha: .10), blurRadius: 34, offset: const Offset(0, 20)),
                        ],
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          Row(
                            children: [
                              IconButton(onPressed: () => Navigator.of(context).pop(), icon: const Icon(Icons.arrow_back_rounded)),
                              const Spacer(),
                              const Hero(
                                tag: 'app-logo',
                                child: CircleAvatar(
                                  radius: 24,
                                  backgroundColor: Color(0xFF0EA5E9),
                                  child: Icon(Icons.factory_rounded, color: Colors.white),
                                ),
                              ),
                              const Spacer(),
                              const SizedBox(width: 48),
                            ],
                          ),
                          const SizedBox(height: 18),
                          Text('Şoför Kaydı', textAlign: TextAlign.center, style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w800)),
                          const SizedBox(height: 8),
                          Text('Demo sevkiyatınız kayıt sonrası otomatik hazırlanır.', textAlign: TextAlign.center, style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.black54)),
                          const SizedBox(height: 24),
                          Row(
                            children: [
                              Expanded(child: TextField(controller: _firstNameController, decoration: const InputDecoration(prefixIcon: Icon(Icons.person_rounded), labelText: 'Ad'))),
                              const SizedBox(width: 12),
                              Expanded(child: TextField(controller: _lastNameController, decoration: const InputDecoration(labelText: 'Soyad'))),
                            ],
                          ),
                          const SizedBox(height: 14),
                          TextField(
                            controller: _emailController,
                            keyboardType: TextInputType.emailAddress,
                            decoration: InputDecoration(prefixIcon: const Icon(Icons.mail_rounded), labelText: 'E-posta', helperText: 'Örn: ad@example.com', errorText: _emailError),
                          ),
                          const SizedBox(height: 14),
                          TextField(
                            controller: _phoneController,
                            keyboardType: TextInputType.phone,
                            inputFormatters: const [PhoneInputFormatter()],
                            decoration: InputDecoration(
                              prefixIcon: const _PhonePrefix(),
                              prefixIconConstraints: const BoxConstraints(minWidth: 76),
                              labelText: 'Telefon',
                              helperText: 'Örn: 532 123 45 67',
                              errorText: _phoneError,
                            ),
                          ),
                          const SizedBox(height: 14),
                          TextField(
                            controller: _passwordController,
                            obscureText: _obscurePassword,
                            decoration: InputDecoration(
                              prefixIcon: const Icon(Icons.lock_rounded),
                              labelText: 'Şifre',
                              suffixIcon: IconButton(
                                onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                                icon: Icon(_obscurePassword ? Icons.visibility_rounded : Icons.visibility_off_rounded),
                              ),
                            ),
                          ),
                          const SizedBox(height: 24),
                          FilledButton(
                            onPressed: auth.isLoading
                                ? null
                                : () async {
                                    final email = ContactValidation.normalizeEmail(_emailController.text);
                                    final phone = ContactValidation.normalizePhone(_phoneController.text);
                                    final error = _validateContact(email, phone);
                                    setState(() {
                                      _emailError = email != null && !ContactValidation.isValidEmail(email) ? ContactValidation.invalidEmailMessage : null;
                                      _phoneError = phone != null && !ContactValidation.isValidPhone(phone) ? ContactValidation.invalidPhoneMessage : null;
                                    });
                                    if (error != null) {
                                      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(error), backgroundColor: const Color(0xFFEF4444)));
                                      return;
                                    }
                                    _phoneController.text = PhoneInputFormatter.format(ContactValidation.phoneInputValue(phone));

                                    await ref.read(authProvider.notifier).register(
                                          firstName: _firstNameController.text.trim(),
                                          lastName: _lastNameController.text.trim(),
                                          email: email,
                                          phoneNumber: phone,
                                          password: _passwordController.text,
                                        );
                                    if (context.mounted && ref.read(authProvider).isAuthenticated) {
                                      Navigator.of(context).pop();
                                    }
                                  },
                            child: AnimatedSwitcher(
                              duration: const Duration(milliseconds: 180),
                              child: auth.isLoading
                                  ? const SizedBox(key: ValueKey('loading'), height: 22, width: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                                  : const Text(key: ValueKey('text'), 'Kayıt Ol'),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  String? _validateContact(String? email, String? phone) {
    if (email == null && phone == null) {
      return ContactValidation.missingContactMessage;
    }
    if (email != null && !ContactValidation.isValidEmail(email)) {
      return ContactValidation.invalidEmailMessage;
    }
    if (phone != null && !ContactValidation.isValidPhone(phone)) {
      return ContactValidation.invalidPhoneMessage;
    }
    return null;
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
