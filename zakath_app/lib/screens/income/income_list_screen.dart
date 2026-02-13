// lib/screens/income/income_list_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';
import 'income_form_screen.dart';

class IncomeListScreen extends ConsumerStatefulWidget {
  const IncomeListScreen({super.key});
  @override ConsumerState<IncomeListScreen> createState() => _IncomeListScreenState();
}

class _IncomeListScreenState extends ConsumerState<IncomeListScreen> {
  @override
  void initState() {
    super.initState();
    SchedulerBinding.instance.addPostFrameCallback((_) => ref.read(incomeListProvider.notifier).fetch());
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(incomeListProvider);
    return Stack(children: [
      state.when(
        data: (list) => list.isEmpty
            ? const Center(child: Text('No income records yet.', style: TextStyle(color: AppColors.textSecondary)))
            : ListView.builder(padding: const EdgeInsets.all(12), itemCount: list.length, itemBuilder: (ctx, i) {
                final item = list[i];
                return Dismissible(
                  key: ValueKey(item.incomeId),
                  direction: DismissDirection.endToStart,
                  background: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.error, borderRadius: BorderRadius.circular(12)),
                    alignment: AlignmentDirectional.centerEnd, padding: const EdgeInsets.only(right: 20),
                    child: const Icon(Icons.delete, color: Colors.white)),
                  onDismissed: (_) => ref.read(incomeListProvider.notifier).remove(item.incomeId),
                  child: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(12),
                      boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 5, offset: const Offset(0, 2))]),
                    child: ListTile(
                      leading: CircleAvatar(backgroundColor: AppColors.incomeColor.withOpacity(0.15),
                        child: const Icon(Icons.arrow_downward, color: AppColors.incomeColor)),
                      title: Text(item.sourceName ?? item.categoryName ?? 'Income', style: const TextStyle(fontWeight: FontWeight.w600)),
                      subtitle: Text('${item.categoryName ?? ''}  •  ${item.hijriDate ?? ''}',
                        style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
                      trailing: Text('${item.currencySymbol ?? '\$'}${item.amount.toStringAsFixed(2)}',
                        style: const TextStyle(color: AppColors.incomeColor, fontWeight: FontWeight.bold, fontSize: 16)),
                    )),
                );
              }),
        loading: () => const Center(child: CircularProgressIndicator(color: AppColors.primary)),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
      Positioned(bottom: 20, right: 20, child: FloatingActionButton(
        backgroundColor: AppColors.incomeColor, foregroundColor: Colors.white,
        onPressed: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const IncomeFormScreen())),
        child: const Icon(Icons.add, size: 28))),
    ]);
  }
}
