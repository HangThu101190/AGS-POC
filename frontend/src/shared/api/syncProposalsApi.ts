import { apiClient } from "@/shared/api/client";

export type SyncProposalDto = {
  id: string;
  flightId: string;
  weekId: string;
  dayIdx: number;
  flightNo: string;
  delayMinutes: number;
  status: string;
  summary: string;
  affected: {
    slotId: string;
    assignmentId?: string | null;
    employeeName: string;
    currentSegments: string[];
    proposedSegments: string[];
  }[];
};

export async function fetchSyncProposals(weekId?: string, pendingOnly = true) {
  const { data } = await apiClient.get<SyncProposalDto[]>("/api/v1/sync-proposals", {
    params: { weekId, pendingOnly },
  });
  return data;
}

export async function confirmSyncProposal(id: string) {
  const { data } = await apiClient.post<SyncProposalDto>(`/api/v1/sync-proposals/${id}/confirm`);
  return data;
}

export async function dismissSyncProposal(id: string) {
  const { data } = await apiClient.post<SyncProposalDto>(`/api/v1/sync-proposals/${id}/dismiss`);
  return data;
}
