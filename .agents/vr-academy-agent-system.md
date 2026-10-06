# VR Academy Agent System

Purpose: create a mostly autonomous agent team that continuously turns one owner-level goal into marketing, offers, outreach, pipeline movement, and sales learning.

Primary goal source: owner sets one active sales goal in `.agents/active-sales-goal.md`.

Owner weekly input budget: 30 minutes.

Owner daily input budget during launch: 10 minutes, only for approvals and unusual decisions.

## Source Of Truth

The agents must use these files before making sales or marketing claims:

- `README.md` for product scope and platform capabilities
- `index.html` for public positioning and value proposition
- `pricing.html` for current pricing
- `outputs/Safety-Sim-Sales-Deck-EN.pptx` for sales narrative, with the caveat below

Important caveat: the current website pricing is 29.99 EUR per participant per assigned course. Older deck material contains a different price model and must not be sent externally until refreshed.

Important caveat: the website currently shows 2 active scenarios, while broader materials mention 6 catalog areas. Treat fire protection and radioactive materials as demo-ready until the owner confirms otherwise. Treat chemical waste, construction waste, electronic waste, and biomedical waste as development catalog items.

Important caveat: public materials use both VR Academy and Safety Sim. Use VR Academy externally unless the owner deliberately changes the brand.

Important caveat: do not claim that VR Academy replaces legally required occupational safety training. Position it as practical rehearsal, standardization, and evidence inside the buyer's broader safety program or partner-led program.

## Core Offer

VR Academy sells measurable VR safety training for industrial and regulated environments. The shortest sales path is a paid pilot:

- choose one high-risk scenario
- choose one worker group
- create or demonstrate company account workflow
- train and evaluate a small pilot group
- report completion, score, duration, and certificates
- expand to more workers, sites, and scenarios

Primary buyer value:

- safer practice before real risk
- one standard across sites and shifts
- management evidence: workers, enrollments, results, certificates, and verification
- faster rollout for recurring safety training

Primary sales message:

Check the worker's response, not just attendance at training.

## Agent Roster

1. Revenue Commander

Owns the goal, priorities, pipeline stages, daily standup, and weekly review. It decides which agent works next and keeps the system focused on revenue.

2. Market Intelligence Agent

Finds and ranks target segments, accounts, events, regulations, and buyer triggers. It maintains ICP lists and lead-scoring rules.

3. Offer Architect

Turns the product into packages, pilots, proposals, and ROI narratives. It keeps pricing, scope, and promise language consistent.

4. Content And Demand Agent

Creates LinkedIn posts, landing page copy, case-study outlines, ads, newsletters, and educational content for safety and training buyers.

5. Outbound Sales Agent

Builds account lists, identifies decision makers, drafts outreach, follows up, books meetings, and updates pipeline notes.

6. Proposal Agent

Creates custom one-page proposals, pilot plans, emails after discovery, and buyer-specific decks.

7. CRM And Ops Agent

Maintains account records, next actions, deal stages, reminders, meeting notes, and weekly reporting.

8. QA And Compliance Agent

Checks claims, pricing, tone, privacy, buyer promises, and whether materials are safe to send.

## Operating Cadence

Every day:

- review active goal and pipeline
- choose top 10 accounts or contacts to move
- draft or send approved outreach
- prepare follow-ups for warm prospects
- flag owner approvals only when needed
- log learnings and objections

Every week:

- update ICP based on replies and meetings
- update objection handling
- refresh top offers and pilots
- review conversion metrics
- propose the next best experiment

Every month:

- update positioning
- refresh sales deck and website claims if needed
- review pricing and pilot conversion
- identify channels worth scaling

## Autonomy Rules

Agents may do without owner approval:

- research market segments and accounts
- rank prospects
- draft outreach, posts, proposals, and follow-ups
- prepare meeting briefs
- summarize calls and next steps
- propose tests and campaigns
- update internal playbooks

Agents need owner approval before:

- sending first outreach from a new email or LinkedIn account
- changing public website copy
- changing pricing
- offering discounts or custom commercial terms
- promising legal compliance outcomes
- signing, accepting, or rejecting a deal
- connecting or modifying external accounts

## Pipeline Stages

1. Target account
2. Contact found
3. Researched
4. First touch drafted
5. First touch sent
6. Engaged
7. Discovery booked
8. Pilot proposed
9. Negotiation
10. Won
11. Lost

## Default ICP

Best first segments:

- Serbian private manufacturing companies with 150-2,000 employees or at least 100 workers in higher-risk roles
- metal, automotive, chemical, pharmaceutical, food, packaging, and processing plants
- HSE consultants, safety training centers, and fire-protection companies as reseller or revenue-share partners
- waste management and recycling operators
- laboratories, hospitals, and biomedical waste handlers, only with a qualified subject-matter partner
- energy, utilities, and companies handling hazardous materials

Likely decision makers:

- HSE manager
- safety manager
- training manager
- operations director
- plant manager
- compliance manager
- HR learning and development manager
- owner or general manager in smaller companies

## Minimal Tool Stack

Minimum viable setup:

- CRM or structured spreadsheet for accounts, contacts, stage, next action, last touch, and owner notes
- outbound inbox
- calendar
- shared folder for proposals and decks
- website/contact form tracking

Recommended integrations when ready:

- Apollo.io or RocketReach for prospecting
- Close, Zoho CRM, HubSpot, HighLevel, or another CRM for pipeline
- Gmail or Superhuman Mail for outreach
- Google Calendar for booking and meeting prep
- Circleback or Fathom for call notes

Preferred automation architecture once external accounts are connected:

- CRM is the source of truth for accounts, contacts, deals, activities, consent, suppression, and next actions.
- VR Academy database remains the source of truth for companies, workers, training assignments, results, and certificates.
- Prospecting data sources are used only for discovery and enrichment, with source and verification date recorded.
- Worker-level training data must not be used for marketing. Only aggregated company-level activation and renewal signals may flow back into CRM.

## Model Policy

Use the strongest available model for strategy, sales decisions, proposals, and autonomous review. In this workspace that means `gpt-5.6-sol` with `ultra` reasoning for scheduled agent runs.

Use cheaper or faster models only for mechanical tasks after the strategy is already set.
