import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/shared/api/client";
import type { HealthResponse } from "@/shared/types/api";

export function useHealth() {
  return useQuery({
    queryKey: ["health"],
    queryFn: async () => {
      const { data } = await apiClient.get<HealthResponse>("/api/v1/health");
      return data;
    },
    refetchInterval: 30_000,
  });
}
