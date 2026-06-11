export type MediaType = 'Movie' | 'TvShow'
export type Role = 'Admin' | 'User'

export interface AuthResponse {
  token: string
  expiresAtUtc: string
  userId: number
  username: string
  email: string
  role: Role
}

export interface GenreResponse {
  id: number
  name: string
  titleCount: number
}

export interface TitleResponse {
  id: number
  mediaType: MediaType
  name: string
  releaseYear: number
  posterUrl: string | null
  backdropUrl: string | null
  averageRating: number | null
  reviewCount: number
  ratingCount: number
  genres: GenreResponse[]
}

export interface CastMember {
  personId: number
  name: string
  characterName: string | null
  profileImageUrl: string | null
  castOrder: number
}

export interface TitleDetail extends TitleResponse {
  tmdbId: number | null
  description: string | null
  director: string | null
  runtimeMinutes: number | null
  createdAt: string
  cast: CastMember[]
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ReviewResponse {
  id: number
  content: string
  createdAt: string
  updatedAt: string | null
  userId: number
  username: string
  titleId: number
  titleName: string
  titlePosterUrl: string | null
}

export interface RatingResponse {
  id: number
  score: number
  createdAt: string
  updatedAt: string | null
  userId: number
  username: string
  titleId: number
  titleName: string
  titlePosterUrl: string | null
}

export interface UserProfile {
  id: number
  username: string
  email: string
  role: Role
  createdAt: string
  reviewCount: number
  ratingCount: number
  reviews: ReviewResponse[]
  ratings: RatingResponse[]
}

export interface TmdbSearchItem {
  tmdbId: number
  mediaType: MediaType
  name: string
  releaseYear: number | null
  overview: string | null
  posterUrl: string | null
  voteAverage: number
  alreadyImported: boolean
  titleId: number | null
}

export interface TitleQuery {
  mediaType?: MediaType
  genreId?: number
  search?: string
  sort?: 'newest' | 'name' | 'year' | 'rating'
  page?: number
  pageSize?: number
}

export interface ProblemDetails {
  status?: number
  title?: string
  detail?: string
}
