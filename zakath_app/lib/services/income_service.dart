// lib/services/income_service.dart
import '../models/models.dart';
import 'api_service.dart';

class IncomeService {
  Future<List<IncomeModel>> getAll() async {
    final res = await apiService.dio.get('/Income');
    final list = (res.data as Map<String, dynamic>)['data'] as List<dynamic>;
    return list.map((e) => IncomeModel.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<IncomeModel> create(IncomeRequest req) async {
    final res = await apiService.dio.post('/Income', data: req.toJson());
    return IncomeModel.fromJson((res.data as Map<String, dynamic>)['data'] as Map<String, dynamic>);
  }

  Future<bool> delete(int id) async {
    final res = await apiService.dio.delete('/Income/$id');
    return (res.data as Map<String, dynamic>)['success'] == true;
  }
}

final IncomeService incomeService = IncomeService();
