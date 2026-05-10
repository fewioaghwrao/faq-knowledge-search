export type AiSource = {
  id: number;
  title: string;
  url: string;
};

export type AiSearchRequest = {
  question: string;
};

export type AiSearchResponse = {
  answer: string | null;
  disclaimer: string | null;
  sources: AiSource[];
  message: string | null;
  aiHistoryId: number;
};