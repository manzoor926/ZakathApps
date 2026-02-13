// lib/services/expense_service.dart
import '../models/models.dart';
import 'api_service.dart';

class ExpenseService {
  Future<List<ExpenseModel>> getAll() async {
    final res = await apiService.dio.get('/Expense');
    final list = (res.data as Map<String, dynamic>)['data'] as List<dynamic>;
    return list.map((e) => ExpenseModel.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<ExpenseModel> create(ExpenseRequest req) async {
    final res = await apiService.dio.post('/Expense', data: req.toJson());
    return ExpenseModel.fromJson((res.data as Map<String, dynamic>)['data'] as Map<String, dynamic>);
  }

  Future<bool> delete(int id) async {
    final res = await apiService.dio.delete('/Expense/$id');
    return (res.data as Map<String, dynamic>)['success'] == true;
  }
}

final ExpenseService expenseService = ExpenseService();
