// lib/screens/dashboard/dashboard_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final statsState  = ref.watch(dashboardStatsProvider);
    final recentState = ref.watch(recentTransactionsProvider);

    return SingleChildScrollView(padding: const EdgeInsets.all(16), child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        // ── Stats cards ──
        statsState.when(
          data: (stats) => Column(children: [
            Row(children: [
              Expanded(child: _statCard('Income',  stats.totalIncome,  AppColors.incomeColor,  Icons.trending_up)),
              const SizedBox(width: 12),
              Expanded(child: _statCard('Expenses', stats.totalExpenses, AppColors.expenseColor, Icons.trending_down)),
            ]),
            const SizedBox(height: 12),
            Row(children: [
              Expanded(child: _statCard('Assets',   stats.totalAssets,  AppColors.primary,      Icons.account_balance)),
              const SizedBox(width: 12),
              Expanded(child: _statCard('Net Worth', stats.netWorth,    AppColors.accent,       Icons.bar_chart)),
            ]),
            const SizedBox(height: 12),
            // Zakath banner
            Container(
              decoration: BoxDecoration(
                color: (stats.isZakathDue ? AppColors.warning : AppColors.success).withOpacity(0.13),
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: stats.isZakathDue ? AppColors.warning : AppColors.success),
              ),
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              child: Row(children: [
                Icon(stats.isZakathDue ? Icons.warning_amber : Icons.check_circle,
                  color: stats.isZakathDue ? AppColors.warning : AppColors.success, size: 26),
                const SizedBox(width: 10),
                Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(stats.isZakathDue ? 'Zakath is Due' : 'Zakath Up to Date',
                    style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15,
                      color: stats.isZakathDue ? AppColors.warning : AppColors.success)),
                  Text(stats.isZakathDue ? 'Due: \$${_f(stats.zakathDue)}' : 'Paid: \$${_f(stats.zakathPaid)}',
                    style: const TextStyle(color: AppColors.textSecondary, fontSize: 13)),
                ]),
              ]),
            ),
          ]),
          loading: () => const Center(child: CircularProgressIndicator(color: AppColors.primary)),
          error: (e, _) => Center(child: Text('Stats error: $e')),
        ),

        const SizedBox(height: 22),
        const Text('Recent Transactions', style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
        const SizedBox(height: 8),

        // ── Recent list ──
        recentState.when(
          data: (list) => list.isEmpty
              ? const Text('No transactions yet.', style: TextStyle(color: AppColors.textSecondary))
              : Column(children: list.map(_txRow).toList()),
          loading: () => const Center(child: CircularProgressIndicator(color: AppColors.primary)),
          error: (e, _) => Center(child: Text('Error: $e')),
        ),
      ],
    ));
  }

  Widget _statCard(String title, double val, Color color, IconData icon) => Container(
    decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(14),
      boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 6, offset: const Offset(0, 2))]),
    padding: const EdgeInsets.all(16),
    child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
        Text(title, style: const TextStyle(color: AppColors.textSecondary, fontSize: 13)),
        Icon(icon, color: color, size: 20),
      ]),
      const SizedBox(height: 6),
      Text('\$${_f(val)}', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: color)),
    ]),
  );

  Widget _txRow(dynamic t) {
    final isIncome = t.type == 'Income';
    final color = isIncome ? AppColors.incomeColor : AppColors.expenseColor;
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(12),
        boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 4, offset: const Offset(0, 1))]),
      child: ListTile(
        leading: CircleAvatar(backgroundColor: color.withOpacity(0.15),
          child: Icon(isIncome ? Icons.arrow_downward : Icons.arrow_upward, color: color, size: 18)),
        title: Text(t.description ?? t.type, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
        subtitle: Text(t.category ?? '', style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
        trailing: Text('${t.currencySymbol ?? '\$'}${_f(t.amount)}',
          style: TextStyle(fontWeight: FontWeight.bold, color: color, fontSize: 15)),
      ),
    );
  }

  String _f(double v) => v.toStringAsFixed(2);
}
