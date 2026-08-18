export interface BrandDto {
  id: string;
  name: string;
  agencyId: string;
  agencyName: string;
  agencyColor: string;
}

export interface CreateBrandRequest {
  name: string;
  agencyId: string;
}

export interface UpdateBrandRequest extends CreateBrandRequest {
  id: string;
}
