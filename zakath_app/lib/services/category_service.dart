// lib/services/category_service.dart
import '../models/models.dart';
import 'api_service.dart';

class CategoryService {
  // GET /api/Category/{type}  →  type = 'Income' or 'Expense'
  Future<List<CategoryModel>> getByType(String type) async {
    final res = await apiService.dio.get('/Category/$type');
    final data = res.data as Map<String, dynamic>;
    final list = data['data'] as List<dynamic>;
    return list.map((e) => CategoryModel.fromJson(e as Map<String, dynamic>)).toList();
  }

  // GET /api/AssetCategory
  Future<List<AssetCategoryModel>> getAssetCategories() async {
    final res = await apiService.dio.get('/AssetCategory');
    final data = res.data as Map<String, dynamic>;
    final list = data['data'] as List<dynamic>;
    return list.map((e) => AssetCategoryModel.fromJson(e as Map<String, dynamic>)).toList();
  }
}

final CategoryService categoryService = CategoryService();
