// lib/main.dart
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'models/models.dart';
import 'providers/auth_provider.dart';
import 'screens/auth/login_screen.dart';
import 'screens/auth/register_screen.dart';
import 'screens/payment/payment_list_screen.dart';
import 'widgets/app_shell.dart';
import 'utils/constants.dart';

void main() {
  runApp(const ProviderScope(child: ZakathApp()));
}

class ZakathApp extends ConsumerStatefulWidget {
  const ZakathApp({super.key});
  @override ConsumerState<ZakathApp> createState() => _ZakathAppState();
}

class _ZakathAppState extends ConsumerState<ZakathApp> {
  bool _checked = false;
  final _navKey = GlobalKey<NavigatorState>();

  @override
  void initState() {
    super.initState();
    SchedulerBinding.instance.addPostFrameCallback((_) async {
      await ref.read(authProvider.notifier).checkAuth();
      if (mounted) setState(() { _checked = true; });
    });
  }

  @override
  Widget build(BuildContext context) {
    final user = ref.watch(authProvider);

    if (!_checked) {
      return MaterialApp(
        home: Scaffold(
          backgroundColor: AppColors.primary,
          body: Center(child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
            const Icon(Icons.mosque, size: 64, color: Colors.white),
            const SizedBox(height: 12),
            const Text('Zakath App', style: TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.bold)),
          ])),
        ),
      );
    }

    return MaterialApp(
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: AppColors.primary),
        useMaterial3: true,
      ),
      initialRoute: user != null ? '/' : '/login',
      routes: {
        '/login':    (_) => LoginScreen(),
        '/register': (_) => RegisterScreen(),
        '/':         (_) => AppShell(),
        '/payments': (_) => PaymentListScreen(),
      },
      navigatorKey: _navKey,
      builder: (context, child) {
        ref.listen<UserModel?>(authProvider, (prev, next) {
          if (next == null && prev != null) {
            _navKey.currentState?.pushNamedAndRemoveUntil('/login', (_) => false);
          }
        });
        return child!;
      },
    );
  }
}