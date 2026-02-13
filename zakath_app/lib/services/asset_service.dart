// lib/services/asset_service.dart
import '../models/models.dart';
import 'api_service.dart';

class AssetService {
  Future<List<AssetModel>> getAll() async {
    final res = await apiService.dio.get('/Asset');
    final list = (res.data as Map<String, dynamic>)['data'] as List<dynamic>;
    return list.map((e) => AssetModel.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<AssetModel> create(AssetRequest req) async {
    final res = await apiService.dio.post('/Asset', data: req.toJson());
    return AssetModel.fromJson((res.data as Map<String, dynamic>)['data'] as Map<String, dynamic>);
  }

  Future<bool> delete(int id) async {
    final res = await apiService.dio.delete('/Asset/$id');
    return (res.data as Map<String, dynamic>)['success'] == true;
  }
}

final AssetService assetService = AssetService();
