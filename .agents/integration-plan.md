# VR Academy Integration Plan

The agent system can plan and draft immediately. To operate closer to 24/7 with low owner dependence, it needs connected external systems.

## Phase 1: Now

- maintain agent playbooks in `.agents`
- generate target lists and drafts
- prepare proposals and follow-ups
- run scheduled reviews
- ask owner only for approvals

## Phase 2: Prospecting

Connect one prospecting data source:

- Apollo.io for B2B prospecting and list building
- RocketReach for finding decision-maker contact details

Goal:

- agents can find accounts and contacts without manual copy-paste

## Phase 3: CRM

Connect one CRM:

- Close if outbound email and sales activity are central
- HubSpot if the main priority is simple pipeline, forms, activity history, and marketing handoff
- Zoho CRM if a general CRM is preferred
- HighLevel if marketing automation and CRM should live together

Goal:

- agents can maintain pipeline, tasks, notes, and follow-ups

## Phase 4: Email And Calendar

Connect:

- Gmail or Superhuman Mail for outreach drafts and replies
- Google Calendar for booking and meeting prep

Goal:

- agents can prepare, send after approval, follow up, and schedule meetings

## Phase 5: Meetings

Connect:

- Fathom, Circleback, or similar call-note system

Goal:

- agents can summarize calls, extract objections, update CRM, and draft proposals automatically

## Owner Approval Defaults

Until email and CRM are connected and approved, agents should draft actions but not send external messages.

Once connected:

- owner approves first campaign per segment
- agent can continue approved follow-ups inside that sequence
- owner approves pricing exceptions and contracts

## Compliance Defaults

- Track opt-outs and never contact opted-out addresses.
- Record lead source and verification date.
- Keep daily outbound volume conservative at launch.
- Pause all automated follow-ups when a person replies.
- Keep promotional email authentication and unsubscribe requirements in place before scaling.
- Do not move individual worker training records into marketing systems.
