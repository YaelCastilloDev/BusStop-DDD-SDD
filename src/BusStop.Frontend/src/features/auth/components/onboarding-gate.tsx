import { useAuth } from '@/lib/adapters/auth'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { OnboardingForm } from '@/features/auth/components/onboarding-form'
import { useMyProfile } from '@/features/auth/hooks/use-my-profile'

// Blocking onboarding gate (SPEC-IdentityAccess-LoginOnboardingFrontend):
// opens after login whenever the local profile still has no username.
// The dialog is non-dismissible — no close button, no click-outside, no Escape —
// until POST /auth/onboarding succeeds and the profile cache is updated.
export function OnboardingGate() {
  const { isAuthenticated, user } = useAuth()
  const profile = useMyProfile()

  const open =
    isAuthenticated &&
    user?.emailVerified === true &&
    profile.data !== undefined &&
    profile.data !== null &&
    profile.data.username === null

  return (
    <Dialog open={open}>
      <DialogContent
        showCloseButton={false}
        onInteractOutside={(event) => event.preventDefault()}
        onEscapeKeyDown={(event) => event.preventDefault()}
        overlayClassName='z-100 bg-black/50 backdrop-blur-sm'
        className='z-100 sm:max-w-md'
        aria-describedby='onboarding-description'
      >
        <DialogHeader>
          <DialogTitle className='text-h3'>Complete your profile</DialogTitle>
          <DialogDescription id='onboarding-description'>
            Choose a username and your country to finish setting up your BusStop
            account.
          </DialogDescription>
        </DialogHeader>

        <OnboardingForm enabled={open} />
      </DialogContent>
    </Dialog>
  )
}
