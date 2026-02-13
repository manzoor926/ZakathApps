// lib/screens/expense/expense_list_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';
import 'expense_form_screen.dart';

class ExpenseListScreen extends ConsumerStatefulWidget {
  const ExpenseListScreen({super.key});
  @override ConsumerState<ExpenseListScreen> createState() => _ExpenseListScreenState();
}

class _ExpenseListScreenState extends ConsumerState<ExpenseListScreen> {
  @override
  void initState() {
    super.initState();
    SchedulerBinding.instance.addPostFrameCallback((_) => ref.read(expenseListProvider.notifier).fetch());
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(expenseListProvider);
    return Stack(children: [
      state.when(
        data: (list) => list.isEmpty
            ? const Center(child: Text('No expense records yet.', style: TextStyle(color: AppColors.textSecondary)))
            : ListView.builder(padding: const EdgeInsets.all(12), itemCount: list.length, itemBuilder: (ctx, i) {
                final item = list[i];
                return Dismissible(
                  key: ValueKey(item.expenseId),
                  direction: DismissDirection.endToStart,
                  background: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.error, borderRadius: BorderRadius.circular(12)),
                    alignment: AlignmentDirectional.centerEnd, padding: const EdgeInsets.only(right: 20),
                    child: const Icon(Icons.delete, color: Colors.white)),
                  onDismissed: (_) => ref.read(expenseListProvider.notifier).remove(item.expenseId),
                  child: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(12),
                      boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 5, offset: const Offset(0, 2))]),
                    child: ListTile(
                      leading: CircleAvatar(backgroundColor: AppColors.expenseColor.withOpacity(0.15),
                        child: const Icon(Icons.arrow_upward, color: AppColors.expenseColor)),
                      title: Text(item.accountDeductedFrom ?? item.categoryName ?? 'Expense', style: const TextStyle(fontWeight: FontWeight.w600)),
                      subtitle: Text('${item.categoryName ?? ''}  •  ${item.hijriDate ?? ''}',
                        style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
                      trailing: Text('${item.currencySymbol ?? '\$'}${item.amount.toStringAsFixed(2)}',
                        style: const TextStyle(color: AppColors.expenseColor, fontWeight: FontWeight.bold, fontSize: 16)),
                    )),
                );
              }),
        loading: () => const Center(child: CircularProgressIndicator(color: AppColors.primary)),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
      Positioned(bottom: 20, right: 20, child: FloatingActionButton(
        backgroundColor: AppColors.expenseColor, foregroundColor: Colors.white,
        onPressed: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ExpenseFormScreen())),
        child: const Icon(Icons.add, size: 28))),
    ]);
  }
}
