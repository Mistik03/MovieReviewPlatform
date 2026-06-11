import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from './client'
import type {
  AuthResponse,
  GenreResponse,
  PagedResult,
  RatingResponse,
  ReviewResponse,
  TitleDetail,
  TitleQuery,
  TitleResponse,
  TmdbSearchItem,
  UserProfile,
} from './types'

// ---------- Titles ----------

export function useTitles(query: TitleQuery) {
  return useQuery({
    queryKey: ['titles', query],
    queryFn: async () =>
      (await api.get<PagedResult<TitleResponse>>('/api/titles', { params: query })).data,
    placeholderData: (prev) => prev,
  })
}

export function useTitle(id: number | undefined) {
  return useQuery({
    queryKey: ['title', id],
    queryFn: async () => (await api.get<TitleDetail>(`/api/titles/${id}`)).data,
    enabled: !!id,
  })
}

// ---------- Genres ----------

export function useGenres() {
  return useQuery({
    queryKey: ['genres'],
    queryFn: async () => (await api.get<GenreResponse[]>('/api/genres')).data,
    staleTime: 5 * 60 * 1000,
  })
}

// ---------- Reviews ----------

export function useReviews(titleId: number | undefined) {
  return useQuery({
    queryKey: ['reviews', titleId],
    queryFn: async () =>
      (await api.get<ReviewResponse[]>('/api/reviews', { params: { titleId } })).data,
    enabled: !!titleId,
  })
}

export function useCreateReview(titleId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (content: string) =>
      (await api.post<ReviewResponse>('/api/reviews', { titleId, content })).data,
    onSuccess: () => invalidateTitleData(queryClient, titleId),
  })
}

export function useUpdateReview(titleId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, content }: { id: number; content: string }) =>
      (await api.put<ReviewResponse>(`/api/reviews/${id}`, { content })).data,
    onSuccess: () => invalidateTitleData(queryClient, titleId),
  })
}

export function useDeleteReview(titleId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => api.delete(`/api/reviews/${id}`),
    onSuccess: () => invalidateTitleData(queryClient, titleId),
  })
}

// ---------- Ratings ----------

export function useRatings(titleId: number | undefined) {
  return useQuery({
    queryKey: ['ratings', titleId],
    queryFn: async () =>
      (await api.get<RatingResponse[]>('/api/ratings', { params: { titleId } })).data,
    enabled: !!titleId,
  })
}

export function useCreateRating(titleId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (score: number) =>
      (await api.post<RatingResponse>('/api/ratings', { titleId, score })).data,
    onSuccess: () => invalidateTitleData(queryClient, titleId),
  })
}

export function useUpdateRating(titleId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, score }: { id: number; score: number }) =>
      (await api.put<RatingResponse>(`/api/ratings/${id}`, { score })).data,
    onSuccess: () => invalidateTitleData(queryClient, titleId),
  })
}

export function useDeleteRating(titleId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => api.delete(`/api/ratings/${id}`),
    onSuccess: () => invalidateTitleData(queryClient, titleId),
  })
}

// ---------- Profile ----------

export function useProfile(enabled: boolean) {
  return useQuery({
    queryKey: ['profile'],
    queryFn: async () => (await api.get<UserProfile>('/api/users/me')).data,
    enabled,
  })
}

// ---------- Auth ----------

export async function loginRequest(usernameOrEmail: string, password: string) {
  return (await api.post<AuthResponse>('/api/auth/login', { usernameOrEmail, password })).data
}

export async function registerRequest(username: string, email: string, password: string) {
  return (await api.post<AuthResponse>('/api/auth/register', { username, email, password })).data
}

// ---------- Admin: TMDB import & catalog management ----------

export function useTmdbSearch(query: string) {
  return useQuery({
    queryKey: ['tmdb-search', query],
    queryFn: async () =>
      (await api.get<TmdbSearchItem[]>('/api/import/tmdb/search', { params: { query } })).data,
    enabled: query.trim().length >= 2,
    staleTime: 60 * 1000,
  })
}

export function useTmdbImport() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (item: { tmdbId: number; mediaType: string }) =>
      (await api.post<TitleDetail>('/api/import/tmdb', item)).data,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['titles'] })
      void queryClient.invalidateQueries({ queryKey: ['genres'] })
      void queryClient.invalidateQueries({ queryKey: ['tmdb-search'] })
    },
  })
}

export function useDeleteTitle() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => api.delete(`/api/titles/${id}`),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['titles'] })
      void queryClient.invalidateQueries({ queryKey: ['genres'] })
    },
  })
}

export function useCreateGenre() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) =>
      (await api.post<GenreResponse>('/api/genres', { name })).data,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['genres'] }),
  })
}

export function useDeleteGenre() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => api.delete(`/api/genres/${id}`),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['genres'] }),
  })
}

function invalidateTitleData(queryClient: ReturnType<typeof useQueryClient>, titleId: number) {
  void queryClient.invalidateQueries({ queryKey: ['reviews', titleId] })
  void queryClient.invalidateQueries({ queryKey: ['ratings', titleId] })
  void queryClient.invalidateQueries({ queryKey: ['title', titleId] })
  void queryClient.invalidateQueries({ queryKey: ['titles'] })
  void queryClient.invalidateQueries({ queryKey: ['profile'] })
}
