// lib/widgets/app_shell.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/auth_provider.dart';
import '../screens/dashboard/dashboard_screen.dart';
import '../screens/income/income_list_screen.dart';
import '../screens/expense/expense_list_screen.dart';
import '../screens/asset/asset_list_screen.dart';
import '../screens/zakath/zakath_screen.dart';
import '../utils/constants.dart';

class AppShell extends ConsumerStatefulWidget {
  const AppShell({super.key});
  @override ConsumerState<AppShell> createState() => _AppShellState();
}

class _AppShellState extends ConsumerState<AppShell> {
  int _idx = 0;

  @override
  Widget build(BuildContext context) {
    final user = ref.watch(authProvider);

    // Pages must live here inside build(), not as a field.
    // Consumer widgets need an active ProviderScope ancestor,
    // which only exists once the widget tree is being built.
    final pages = [
      const DashboardScreen(),
      const IncomeListScreen(),
      const ExpenseListScreen(),
      const AssetListScreen(),
      const ZakathScreen(),
    ];

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        title: Text(user?.fullName ?? 'Zakath'),
        actions: [
          IconButton(icon: const Icon(Icons.logout),
            onPressed: () => ref.read(authProvider.notifier).logout()),
        ],
      ),
      body: pages[_idx],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _idx,
        onDestinationSelected: (i) => setState(() { _idx = i; }),
        backgroundColor: Colors.white,
        indicatorColor: AppColors.primary.withOpacity(0.12),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.dashboard),       label: 'Dashboard'),
          NavigationDestination(icon: Icon(Icons.trending_up),     label: 'Income'),
          NavigationDestination(icon: Icon(Icons.trending_down),   label: 'Expense'),
          NavigationDestination(icon: Icon(Icons.account_balance), label: 'Assets'),
          NavigationDestination(icon: Icon(Icons.mosque),          label: 'Zakath'),
        ],
      ),
    );
  }
}