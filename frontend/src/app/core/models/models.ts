export enum UserRole {
  Admin = 0,
  FullExperience = 1,
  LimitedExperience = 2,
}

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  accessCode?: string;
  role: UserRole;
  maxCompanions?: number | null;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email?: string;
  password?: string;
  role: UserRole;
}

export interface AdminLoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenDto {
  token: string;
  expires: string;
  created: string;
}

export interface LoginResponseDto {
  token: string;
  refreshToken: RefreshTokenDto;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface GuestCompanion {
  id: string;
  firstName: string;
  lastName: string;
  age?: number;
  dietaryRestrictions?: string;
  notes?: string;
  guestType: string;
}

export interface RsvpDto {
  id: string;
  userId: string;
  isAttending: boolean;
  respondedAt?: string;
  companions: GuestCompanion[];
  dietaryRestrictions?: string;
  notes?: string;
  contactEmail?: string;
  createdAt: string;
  updatedAt?: string;
  totalGuests: number;
  maxCompanionsAllowed: number;
}

export interface RsvpWithUser {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  isAttending: boolean;
  respondedAt?: string;
  companions: GuestCompanion[];
  dietaryRestrictions?: string;
  notes?: string;
  totalGuests: number;
}

export interface RsvpSummary {
  totalInvited: number;
  totalResponded: number;
  totalAttending: number;
  totalNotAttending: number;
  totalPeople: number;
  totalCompanions: number;
  pendingResponses: number;
  attendingGuests: RsvpWithUser[];
  notAttendingGuests: RsvpWithUser[];
  pendingGuests: RsvpWithUser[];
}

export interface GiftReservation {
  id: string;
  giftId: string;
  reservedByUserId: string;
  reservedByName: string;
  reservedAt: string;
  notes?: string;
}

export interface GiftDto {
  id: string;
  name: string;
  description?: string;
  price?: number;
  imageUrl?: string;
  purchaseLink?: string;
  maxReservations?: number;
  reservationCount: number;
  remainingReservations?: number;
  isFullyReserved: boolean;
  displayOrder: number;
  isVisible: boolean;
  isReservedByMe: boolean;
  reservations: GiftReservation[];
  reservationStatus: string;
}

export interface CreateGiftDto {
  name: string;
  description?: string;
  price?: number;
  imageUrl?: string;
  purchaseLink?: string;
  maxReservations?: number;
  displayOrder: number;
  isVisible: boolean;
}

export interface UpdateGiftDto {
  name: string;
  description?: string;
  price?: number;
  imageUrl?: string;
  purchaseLink?: string;
  maxReservations?: number;
  displayOrder: number;
  isVisible: boolean;
}

export interface ImportGiftsResult {
  imported: number;
  skipped: number;
  errors: string[];
}

export interface LocationDto {
  name?: string;
  address?: string;
  latitude?: number;
  longitude?: number;
  googleMapsUrl?: string;
  appleMapsUrl?: string;
}

export interface WeddingInfoDto {
  userRole?: UserRole;
  brideName: string;
  brideSurname: string;
  groomName: string;
  groomSurname: string;
  approximateDate: string;
  weddingName: string;
  weddingDescription: string;
  weddingDate?: string;
  locationCivil?: LocationDto;
  locationChurch?: LocationDto;
  locationParty?: LocationDto;
  locationHouse?: LocationDto;
}

export interface WeddingInfoUpdateDto {
  brideName: string;
  brideSurname: string;
  groomName: string;
  groomSurname: string;
  approximateDate: string;
  weddingName?: string;
  weddingDescription?: string;
  weddingDate?: string;
  civilLocationName?: string;
  civilLocationAddress?: string;
  civilLocationLatitude?: number;
  civilLocationLongitude?: number;
  civilLocationGoogleMapsUrl?: string;
  civilLocationAppleMapsUrl?: string;
  churchLocationName?: string;
  churchLocationAddress?: string;
  churchLocationLatitude?: number;
  churchLocationLongitude?: number;
  churchLocationGoogleMapsUrl?: string;
  churchLocationAppleMapsUrl?: string;
  partyLocationName?: string;
  partyLocationAddress?: string;
  partyLocationLatitude?: number;
  partyLocationLongitude?: number;
  partyLocationGoogleMapsUrl?: string;
  partyLocationAppleMapsUrl?: string;
  houseLocationName?: string;
  houseLocationAddress?: string;
  houseLocationLatitude?: number;
  houseLocationLongitude?: number;
  houseLocationGoogleMapsUrl?: string;
  houseLocationAppleMapsUrl?: string;
}

export interface ModulesDto {
  gifts: boolean;
  rsvp: boolean;
  reminders: boolean;
}

export interface GuestLoginRequest {
  accessCode: string;
}

export interface CreateGuestCompanionDto {
  firstName: string;
  lastName: string;
  age?: number;
  dietaryRestrictions?: string;
  notes?: string;
}

export interface CreateRsvpDto {
  isAttending: boolean;
  companions: CreateGuestCompanionDto[];
  contactEmail?: string;
  dietaryRestrictions?: string;
  notes?: string;
}

export interface ReserveGiftDto {
  notes?: string;
}

export interface GiftReservationConfirmation {
  giftId: string;
  giftName: string;
  reservedAt: string;
  notes?: string;
}

export enum ReminderUnit {
  Days = 0,
  Weeks = 1,
  Months = 2,
}

export interface ReminderDto {
  id: string;
  value: number;
  unit: ReminderUnit;
  note?: string;
  overrideEmail?: string;
  scheduledFor: string;
  isSent: boolean;
  createdAt: string;
}

export interface AddReminderDto {
  value: number;
  unit: ReminderUnit;
  note?: string;
  overrideEmail?: string;
}
