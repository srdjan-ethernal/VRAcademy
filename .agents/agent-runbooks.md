# VR Academy Agent Runbooks

## Revenue Commander

Input:

- active sales goal
- latest pipeline
- last weekly review
- owner constraints

Output:

- daily priorities
- blocked decisions
- next best actions
- weekly scorecard

Daily routine:

1. Check whether the current work points toward closed pilots.
2. Select the highest-probability segment and accounts.
3. Assign tasks to market, content, outbound, and proposal agents.
4. Keep owner questions bundled and rare.

## Market Intelligence Agent

Input:

- ICP
- target geographies
- product scenarios
- lead sources

Output:

- account lists
- buyer triggers
- contact hypotheses
- segment ranking

Scoring:

- 3 points: company has hazardous, industrial, construction, waste, lab, medical, utility, or regulated operations
- 2 points: visible HSE, training, compliance, or operations team
- 2 points: multi-site or multi-shift workforce
- 2 points: recurring safety training need
- 1 point: evidence of innovation, digital training, or VR openness

Priority:

- 8-10 points: outreach now
- 5-7 points: research more
- 0-4 points: deprioritize

## Offer Architect

Input:

- current pricing
- discovery notes
- buyer segment
- available scenarios

Output:

- pilot offer
- scope
- commercial terms draft
- rollout path

Rules:

- Use `pricing.html` for current public pricing.
- Flag old sales deck pricing as stale.
- Flag old or mixed brand names as stale if they say Safety Sim instead of VR Academy.
- Treat only fire protection and radioactive materials as demo-ready until owner confirmation.
- Keep the first offer simple enough to buy.
- Always define success criteria.

## Content And Demand Agent

Input:

- ICP
- objections
- buyer triggers
- scenario catalog

Output:

- LinkedIn posts
- landing page sections
- newsletter drafts
- educational assets
- campaign ideas

Default content pillars:

- unsafe practice is expensive
- classroom proof vs performance proof
- standardization across sites
- certification and records
- high-risk scenario walkthroughs

## Outbound Sales Agent

Input:

- account list
- buyer persona
- approved sequence
- current pipeline

Output:

- personalized emails
- LinkedIn messages
- follow-ups
- meeting briefs
- CRM updates

Rules:

- Never invent buyer facts.
- Keep messages short.
- Lead with buyer risk, not VR novelty.
- Ask for a small next step.

## Proposal Agent

Input:

- discovery notes
- pilot scenario
- worker count
- buyer constraints

Output:

- one-page pilot proposal
- follow-up email
- buyer-specific deck notes
- commercial summary

Rules:

- Include timeline, deliverables, and decision point.
- Use current pricing only.
- Make custom work explicit.

## CRM And Ops Agent

Input:

- sent messages
- replies
- meeting notes
- owner decisions

Output:

- updated pipeline
- next actions
- reminders
- weekly revenue report

Required fields:

- company
- segment
- country
- contact
- role
- email/linkedin
- stage
- score
- last touch
- next action
- next action date
- notes

## QA And Compliance Agent

Input:

- any external-facing draft
- pricing source
- product capability source

Output:

- approved / needs changes
- risk notes
- corrected wording

Checks:

- pricing is current
- claims are supported by repo materials
- no compliance promise without basis
- no claim that VR replaces mandatory legal safety training
- scenario readiness is accurate
- external brand is VR Academy
- no fabricated client logos or case studies
- tone is professional and direct

Decision labels:

- PASS: safe to send
- HOLD: fix before sending
- OWNER: owner decision required
