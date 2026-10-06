# Bumper Seed — Final Concept & Production Gate

Status: **FINAL CORE / R19 PRODUCTION START**
Date: 2026-10-07

## 1. Locked core loop

1. The ball moves automatically.
2. The player uses one input: **tap**.
3. A tap timed around a wall collision creates a bumper at the impact point.
4. Only the **latest 3 bumpers** remain.
5. Creating a fourth bumper automatically removes the oldest bumper.
6. Difficulty grows through target placement and bumper replacement decisions, not by extreme ball-speed increases.

## 2. Approved depth layers

- Highlight the bumper that will be deleted next.
- Sequential target zones that reward planning several reflections ahead.
- A one-use enhancement for the next created bumper after a successful target chain.
- Reusable target/pattern library with controlled variation.
- Strong collision/creation feedback: animation, sound, score, and short visual emphasis.

## 3. Explicitly rejected for the core

- Permanent 4-bumper management.
- Unlimited bumpers.
- Large late-game ball-speed spikes as the primary difficulty system.
- Extra action buttons or swipe commands.
- Punishing a missed perfect-timing bonus.
- Hidden bumper lifetime timers.

## 4. R19 — Playable core prototype

R19 exists to answer one question:

> Is the locked one-tap / three-bumper loop actually fun and readable on a phone?

Required R19 behavior:

- Portrait mobile arena.
- Automatic ball motion.
- Zero-gravity 2D bounce.
- Buffered collision-tap window to avoid frame-perfect input.
- Bumper spawn at the most recent wall impact.
- FIFO maximum of 3 bumpers.
- Oldest bumper visually distinguished.
- One active sequential target.
- Score/combo feedback.
- Stable ball speed rather than escalating speed.
- Separate Android package so the KaiChe RPG build is not overwritten.

R19 is intentionally isolated from the RPG runtime and production scenes.

## 5. R19 acceptance gate

Do **not** call R19 validated from the prior virtual simulations.

R19 passes only after a real device build confirms:

- Tap timing is understandable within the first few attempts.
- The player can predict which bumper disappears next.
- Three bumpers remain readable during motion.
- Target chasing creates deliberate placement decisions.
- No major collision tunneling, stuck-ball, or runaway-speed behavior.
- Repeated play still feels controllable rather than random.

## 6. Next production sequence

- **R19:** playable core prototype.
- **R20:** impact feel + audio/visual feedback tuning.
- **R21:** sequential target patterns + next-bumper enhancement.
- **R22:** difficulty curve and 20+ real-play-session evaluation.
- **R23:** retention/score loop only if the core has passed real play testing.

## 7. Simulation note

Earlier 20-run comparisons were **virtual design simulations**, not executed gameplay telemetry. They are used only to prioritize what to prototype. Real-device results override them.
