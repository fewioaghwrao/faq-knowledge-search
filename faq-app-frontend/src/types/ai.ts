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

export type AiSearchHistoryListItem = {
  id: number;
  question: string;
  answerPreview?: string | null;
  isSuccess: boolean;
  errorMessage?: string | null;
  isHelpful?: boolean | null;
  sourceCount: number;
  executedAt: string;
};