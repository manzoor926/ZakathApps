// lib/screens/asset/asset_list_screen.dart
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/providers.dart';
import '../../utils/constants.dart';
import 'asset_form_screen.dart';

class AssetListScreen extends ConsumerStatefulWidget {
  const AssetListScreen({super.key});
  @override ConsumerState<AssetListScreen> createState() => _AssetListScreenState();
}

class _AssetListScreenState extends ConsumerState<AssetListScreen> {
  @override
  void initState() {
    super.initState();
    SchedulerBinding.instance.addPostFrameCallback((_) => ref.read(assetListProvider.notifier).fetch());
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(assetListProvider);
    return Stack(children: [
      state.when(
        data: (list) => list.isEmpty
            ? const Center(child: Text('No assets yet.', style: TextStyle(color: AppColors.textSecondary)))
            : ListView.builder(padding: const EdgeInsets.all(12), itemCount: list.length, itemBuilder: (ctx, i) {
                final item = list[i];
                return Dismissible(
                  key: ValueKey(item.assetId),
                  direction: DismissDirection.endToStart,
                  background: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.error, borderRadius: BorderRadius.circular(12)),
                    alignment: AlignmentDirectional.centerEnd, padding: const EdgeInsets.only(right: 20),
                    child: const Icon(Icons.delete, color: Colors.white)),
                  onDismissed: (_) => ref.read(assetListProvider.notifier).remove(item.assetId),
                  child: Container(margin: const EdgeInsets.symmetric(vertical: 6),
                    decoration: BoxDecoration(color: AppColors.cardBg, borderRadius: BorderRadius.circular(12),
                      boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 5, offset: const Offset(0, 2))]),
                    child: ListTile(
                      leading: CircleAvatar(backgroundColor: AppColors.primary.withOpacity(0.15),
                        child: const Icon(Icons.diamond, color: AppColors.primary)),
                      title: Text(item.itemName, style: const TextStyle(fontWeight: FontWeight.w600)),
                      subtitle: Text('Qty: ${item.quantity}  •  ${item.isZakathApplicable ? "Zakathable" : "Not Zakathable"}',
                        style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
                      trailing: Column(mainAxisAlignment: MainAxisAlignment.center, crossAxisAlignment: CrossAxisAlignment.end, children: [
                        Text('Now: ${item.currencySymbol ?? '\$'}${item.currentValue.toStringAsFixed(2)}',
                          style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold, fontSize: 14)),
                        Text('Cost: ${item.currencySymbol ?? '\$'}${item.purchaseValue.toStringAsFixed(2)}',
                          style: const TextStyle(color: AppColors.textSecondary, fontSize: 11)),
                      ]),
                    )),
                );
              }),
        loading: () => const Center(child: CircularProgressIndicator(color: AppColors.primary)),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
      Positioned(bottom: 20, right: 20, child: FloatingActionButton(
        backgroundColor: AppColors.primary, foregroundColor: Colors.white,
        onPressed: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const AssetFormScreen())),
        child: const Icon(Icons.add, size: 28))),
    ]);
  }
}
