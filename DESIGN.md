# Step Pet - Unity Mobile App

## Overview
A cute, pastel-colored virtual pet app where walking earns currency (food) to maintain and collect baby animals. Users have a permanent first pet plus can work toward unlocking additional animals through step goals. Social features let friends visit each other's spaces and donate steps.

## Tech Stack
- Unity (latest LTS)
- C#
- Android: Google Fit / Health Connect API for step tracking
- iOS: Apple HealthKit for step tracking
- Backend: Firebase (Auth, Firestore, Cloud Functions) for social features and cloud save
- Portrait orientation only

## Visual Style
- Soft pastel color palette
- Cute baby animals (puppies, baby giraffes, kittens, bunnies, etc.)
- Customizable outdoor scenes (fields, forests, beaches)
- Collectible accessories (hats, toys, backgrounds, decorations)
- Art assets will be provided separately - use placeholder sprites for now

---

## Account System

### Registration (First Launch)
- User picks a username
- Check against Firebase for uniqueness
- Account created and saved immediately
- Short friend code auto-generated (separate from username)

### Recovery
- Optional email in settings
- Prompt after onboarding: "Add email for account recovery?" (skippable)
- If email added: can recover account on new device via email link
- If no email: device-linked only, lose phone = lose account
- Can add/change recovery email anytime in settings

---

## Core Economy

### Currency: Food
- Earned by walking at 1:1 ratio (1 step = 1 food)
- Displayed as a "wallet" that accumulates saved food
- Daily reset at midnight local time

### Daily Quota System
- Each owned animal has a daily food requirement (e.g., puppy = 500 steps/day, baby giraffe = 800 steps/day)
- Total daily requirement = sum of all owned animals' requirements
- NO PENALTY for not meeting quota - animals stay happy regardless
- Animals are EXTRA HAPPY when fed (visual feedback: bouncing, hearts, sparkles)
- You cannot spend/save food until daily quota is met
- Overflow steps (above quota) go into your wallet for spending

### Example Flow
1. User has 2 pets requiring 1000 food/day total
2. User walks 5000 steps today
3. Progress bar shows "5000/1000 food (daily)" and turns green
4. 4000 overflow food animates from the daily bar into the wallet
5. Wallet can be spent on hats, decorations, or saved for new animals

---

## Pet System

### Main Pet (Permanent)
- Chosen at onboarding - this is your first pet
- Can never be lost or changed
- User names it
- Has daily food requirement

### Unlockable Pets
- One unlock attempt at a time
- Each has a STRETCH GOAL: e.g., "20,000 steps in 14 days"
- Progress tracked separately from daily quota (only overflow steps count toward unlock)
- If completed: pet permanently joins your collection (with its own daily requirement now added)
- If failed OR abandoned:
  - Keep 10% of progress (stacking - so repeated attempts accumulate)
  - 2 week cooldown before you can attempt that animal again
- No maximum number of pets - having tons of pets with huge daily quota is the goal
- Pets cannot be released once owned

### Pet Behaviors
- Roam around the scene area
- React happily when fed (extra happy animations)
- Idle animations
- Wear equipped accessories (universal - any accessory fits any animal)

---

## Screens & Navigation

### Navigation Flow
```
[Friends List] <--> [Scene Overview] <--> [Animal Close-ups]
                          ^
                          |
                    LEFT from ANY animal
```

**From Friends List:**
- Swipe RIGHT → Scene Overview

**From Scene Overview:**
- Swipe LEFT → Friends List
- Swipe RIGHT → Animal close-up (first or last viewed)
- Tap any pet → Jump to that pet's close-up

**From Any Animal Close-up:**
- Swipe RIGHT → Next animal
- Swipe LEFT → **Always** returns to Scene Overview (not previous animal)

---

### 1. Pet Close-up Screen
- Full-screen view of one pet in their environment
- Pet's name displayed in a circle badge
- If pet is "in progress" (being unlocked): badge shows progress bar instead (e.g., "1,000/20,000")

**On-screen UI:**
- Top: Daily quota bar (horizontal)
  - Shows "600/1000 food (daily)"
  - Turns green when complete
- Top-right: Wallet/saved food amount
- When opening app: animate overflow food from daily bar to wallet
- Bottom: Expandable menu (swipe up) - shows **Expenditure/Pets**

---

### 2. Scene Overview Screen
- Zoomed out view of entire customizable area
- All owned pets visible, roaming around
- Background shows equipped backdrop (field/forest/beach/etc.)
- Placed decorations and toys visible
- Tap any pet to jump to their close-up
- Same top UI bars as close-up
- Bottom: Expandable menu (swipe up) - shows **Shop**

---

### 3. Friends List Screen
- List of added friends
- Each friend shows:
  - Profile pic/avatar or main pet preview
  - Username
  - Button to visit their space
- Option to add friends via friend code
- Settings button (access to recovery email, notifications, etc.)

---

### 4. Friend's Space (View Mode)
- See their scene with all their pets and decorations
- Read-only (can't modify)
- Option to donate any amount of food from your wallet (no limits)
- See what collectibles they have equipped

---

### 5. Bottom Menu - Context Sensitive

#### When on Pet Close-up Screen: Expenditure View
Covers full screen when expanded.

**Top Section - Expenditure Overview:**
- Total daily requirement bar (sum of all pets)
- Breakdown list, each row:
  - [Pet Picture] [Pet Name] [Daily Requirement: 500/day]
  - Tap any pet → closes menu, zooms to that pet

**Unlock Section (if actively working on one):**
- [Pet Picture] [Pet Name] "In Progress"
- Progress bar: 12,000/20,000 steps
- Time remaining bar: 13 days left
- Option to abandon (confirms with warning about 10% kept + 2 week cooldown)

---

#### When on Scene Overview Screen: Shop View
Covers full screen when expanded. Vertical scrolling.

**Section 1: Hats & Accessories** (3x3 grid)
- Item thumbnail + price
- "Owned" badge if already purchased

**Section 2: Backdrops** (3x3 grid)
- Field, forest, beach, etc.
- "Equipped" badge on current one

**Section 3: Decorations & Toys** (3x3 grid)
- Items to place in scene

**Section 4: New Animal Challenges** (3x3 grid)
- Animals available to work toward
- Shows: thumbnail, step goal, time limit (e.g., "20,000 steps / 14 days")
- "Owned" badge if already unlocked
- "Cooldown: 12 days" if recently failed/abandoned
- Shows any banked progress from previous attempts (e.g., "2,000 steps banked")
- Tap to start challenge (if not already doing one)

**Tapping any purchasable item:** Purchase confirmation popup with price

---

## Notifications

### Walk Reminders
- Evening reminder if daily quota not yet met
- Default time: 7pm local (configurable in settings)
- Example: "Your pets are getting hungry! 400 steps to go"
- Can disable in settings

### Challenge Reminders
- 3 days remaining: "3 days left to unlock Baby Giraffe! 8,000 steps to go"
- 1 day remaining: "Final day for Baby Giraffe! You can do it!"
- Challenge complete: "Baby Giraffe is yours forever!"
- Challenge failed: "Baby Giraffe went home... but you banked 2,000 steps for next time"

### Social Notifications
- "[Username] sent you 500 food!"

### Streak Notifications (nice-to-have)
- "You've hit your quota 7 days in a row!"
- "2 week streak! Your pets love you!"

### Settings
- Toggle all notifications on/off
- Toggle categories individually (walk reminders, challenge, social)
- Set reminder time for walk reminders

---

## Social Features

### Friends System
- Add friends via friend code (short alphanumeric code)
- View friends list with preview of their main pet
- Visit friend's space (read-only view)

### Donations
- Send any amount of food from your wallet to friends (no limits)
- Friend receives push notification: "[Username] sent you 500 food!"
- Donated food goes straight to their wallet

---

## Data Model (Firebase Firestore)
```
users/
  {userId}/
    profile:
      username: string (unique)
      friendCode: string (unique, auto-generated)
      recoveryEmail: string (optional)
      createdAt: timestamp

    settings:
      notificationsEnabled: boolean
      walkReminderEnabled: boolean
      walkReminderTime: string (HH:MM)
      challengeNotificationsEnabled: boolean
      socialNotificationsEnabled: boolean

    economy:
      wallet: number (saved food)
      lifetimeSteps: number
      lastSyncedAt: timestamp
      todaySteps: number
      todayDate: string (YYYY-MM-DD)

    pets/
      {petId}/
        animalType: string (puppy, giraffe, etc.)
        name: string
        isMainPet: boolean
        isOwned: boolean
        dailyRequirement: number
        equippedAccessories: string[]
        unlockedAt: timestamp (null if not owned)

        # If in-progress:
        unlockGoal: number (e.g., 20000)
        unlockProgress: number (includes banked progress)
        unlockDeadline: timestamp

        # If on cooldown:
        cooldownUntil: timestamp
        bankedProgress: number (10% from each failed attempt, stacks)

    inventory/
      hats: string[]
      decorations: string[]
      backdrops: string[]
      toys: string[]

    scene:
      currentBackdrop: string
      placedItems: [{ itemId, position: {x, y} }]

    friends: string[] (userIds)

    notifications/
      {notificationId}/
        type: "donation"
        fromUser: string
        fromUsername: string
        amount: number
        timestamp: timestamp
        read: boolean

    stats:
      currentStreak: number
      longestStreak: number
      totalDaysQuotaMet: number

    dailyProgress/
      {date YYYY-MM-DD}/
        stepsRecorded: number
        quotaMet: boolean
        overflowEarned: number
```

---

## Step Tracking Integration

### Android (Health Connect)
- Use Unity plugin for Health Connect API
- Request step count permission
- Query steps since last sync

### iOS (HealthKit)
- Use Unity plugin for HealthKit
- Request step count permission
- Query step data since last sync

### Sync Logic
1. On app open: check if date changed (reset daily progress if new day at midnight local)
2. Query steps since lastSyncedAt
3. Calculate new steps earned
4. Apply to daily quota first
5. Overflow goes to wallet AND unlock progress (if applicable)
6. Update lastSyncedAt
7. Animate the food flowing from daily bar to wallet
8. Schedule/update notifications based on current progress

---

## Onboarding Flow
1. Welcome screen (app name, cute graphic)
2. Pick username (check availability)
3. Connect health/step tracking (request permissions)
4. Choose your first pet (main pet - explain it's permanent)
5. Name your pet
6. Brief tutorial: explain daily quota, walking = food, overflow = spending money
7. Optional: "Add recovery email?" (can skip, remind later)
8. Enable notifications prompt
9. Drop into main close-up screen

---

## Unlock Challenge Flow

### Starting a Challenge
1. User opens shop (zoom out → swipe up menu)
2. Scrolls to "New Animal Challenges"
3. Taps an animal (e.g., Baby Giraffe - 20,000 steps in 14 days)
4. If banked progress exists, shows "You have 2,000 steps banked from before!"
5. Confirmation: "Start challenge?" → Yes
6. Challenge begins, deadline set to 14 days from now
7. New pet appears in scene/rotation with progress badge instead of name

### During Challenge
- Only overflow steps (above daily quota) count toward challenge
- Progress bar visible on pet badge and in expenditure menu
- Can abandon anytime (keeps 10%, 2 week cooldown)
- Notifications at 3 days and 1 day remaining

### Challenge Success
- Push notification: "Baby Giraffe is yours forever!"
- Celebration animation in app
- Pet's badge changes from progress bar to their name
- Pet permanently added to collection
- Their daily requirement now adds to total quota

### Challenge Failure/Abandon
- Push notification (sympathetic, not punishing)
- "Baby Giraffe went home... but you banked 2,000 steps for next time"
- 10% of progress saved as banked progress (stacks with previous banks)
- Pet enters 2 week cooldown
- Pet disappears from scene until cooldown ends and user tries again

---

## Key Animations
- Food particles flowing from daily bar to wallet on app open
- Pets bouncing/hearts/sparkles when daily quota met
- Progress bars filling smoothly
- Swipe transitions between screens (horizontal slide)
- Pet idle animations and roaming in scene
- Celebration confetti on unlock success
- Gentle fade for failed challenge
- Subtle glow/pulse on actionable UI elements

---

## MVP Feature Priority

### Phase 1 - Core Loop
1. Firebase setup + username registration
2. Step tracking integration (Health Connect + HealthKit)
3. Daily quota system with midnight reset
4. Main pet selection and naming
5. Single pet close-up screen with UI bars
6. Wallet accumulation from overflow
7. Basic happy animations when quota met

### Phase 2 - Scene & Navigation
8. Scene overview with roaming pets
9. Swipe navigation between views
10. Tap to zoom functionality
11. Bottom menu (expenditure view on close-up)

### Phase 3 - Collection
12. Pet unlock challenge system
13. Banked progress + cooldown mechanics
14. Shop UI (bottom menu on scene overview)
15. Purchasable animals

### Phase 4 - Customization
16. Hats/accessories (universal)
17. Background scenes
18. Placeable decorations
19. Scene editor

### Phase 5 - Social
20. Friend codes + adding friends
21. Friends list screen
22. Visiting friend spaces
23. Donation system

### Phase 6 - Polish
24. Push notifications (walk reminders, challenges, social)
25. Notification settings
26. Recovery email option
27. Streak tracking
28. Onboarding tutorial refinements

---

## Placeholder Assets Needed
Create simple colored shapes or use free assets until real art is ready:

**Starter Pets (choose one at onboarding):**
- Puppy (daily req: 300)
- Kitten (daily req: 300)
- Bunny (daily req: 400)

**Unlock Challenge Pets:**
- Duckling (goal: 8,000 / 14 days, daily req: 400)
- Baby Giraffe (goal: 15,000 / 14 days, daily req: 600)
- Baby Elephant (goal: 20,000 / 14 days, daily req: 800)
- Baby Penguin (goal: 25,000 / 14 days, daily req: 700)
- Baby Panda (goal: 35,000 / 14 days, daily req: 1000)

**Accessories (universal):**
- Top hat (500 food)
- Bow (300 food)
- Flower crown (400 food)
- Sunglasses (350 food)
- Party hat (250 food)

**Backdrops:**
- Sunny field (free, default)
- Forest (1000 food)
- Beach (1500 food)

**Decorations:**
- Ball (200 food)
- Flower pot (300 food)
- Bench (500 food)
- Small tree (600 food)

---

## Settings Screen (accessed from Friends List)

- **Account**
  - Username (display only)
  - Friend code (display + copy button)
  - Recovery email (add/change)

- **Notifications**
  - Master toggle
  - Walk reminders (toggle + time picker)
  - Challenge reminders (toggle)
  - Friend notifications (toggle)

- **About**
  - Version number
  - Credits
  - Privacy policy link
  - Terms of service link
