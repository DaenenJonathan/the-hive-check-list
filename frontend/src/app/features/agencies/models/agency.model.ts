export interface AgencyDto {
  id: string;
  name: string;
  color: string;
  brandCount: number;
}

export interface CreateAgencyRequest {
  name: string;
  color: string;
}

export interface UpdateAgencyRequest extends CreateAgencyRequest {
  id: string;
}
