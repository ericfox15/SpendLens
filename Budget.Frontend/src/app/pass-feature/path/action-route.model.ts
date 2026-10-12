/**
 * THE most important thing to learn from this codebase — mirrors eMAR's action-route pipeline.
 *
 * Instead of hardcoding "show a note modal before Pay", the app loads JSON from the server
 * defining WHICH steps to run for PAY, SKIP, and RESET. This means you can add/remove
 * confirmation dialogs without deploying new code — just update the Setting table.
 *
 * Step types:
 *  CHECK_*   = decision points (run remaining steps only if condition is met)
 *  GET_*     = workflow steps (open modals, collect user input, merge into payload)
 *  SUBMIT_*  = final submission
 *  NOTIFICATION_SUCCESS = show toast
 *  KILL      = abort the pipeline
 */

export type CheckType    = 'CHECK_OVER_BUDGET' | 'CHECK_LARGE_PAYMENT' | 'CHECK_REQUIRE_NOTE';
export type WorkflowType = 'GET_NOTE' | 'GET_SKIP_REASON' | 'GET_AMOUNT' | 'QUEUE_ADD_CONFIRMATION';
export type FinishType   = 'SUBMIT_ONLINE' | 'SUBMIT_OFFLINE' | 'SUBMIT_BOTH';
export type ExtraType    = 'NOTIFICATION_SUCCESS';
export type KillType     = 'KILL';
export type StepType = CheckType | WorkflowType | FinishType | ExtraType | KillType;

export interface ActionRoute {
  name: string;
  steps: StepType[];
}

export interface StepPayload {
  paymentId: number;
  amount?: number;
  note?: string | null;
  skipReason?: string;
  cancelled?: boolean;
}

/**
 * Validates an ActionRoute configuration:
 *  - Must have at least one step
 *  - Must end with a SUBMIT_* or KILL step
 *  - Nothing after the terminal step
 *  - Every CHECK has its matching confirmation guard
 */
export function isValidActionRoute(route: ActionRoute): boolean {
  if (!route.steps.length) return false;

  const terminalTypes: StepType[] = ['SUBMIT_ONLINE', 'SUBMIT_OFFLINE', 'SUBMIT_BOTH', 'KILL'];
  const lastStep = route.steps[route.steps.length - 1];
  if (!terminalTypes.includes(lastStep)) return false;

  // CHECK_LARGE_PAYMENT must be followed by QUEUE_ADD_CONFIRMATION before the terminal step
  const checkLarge = route.steps.indexOf('CHECK_LARGE_PAYMENT');
  if (checkLarge >= 0) {
    const hasGuard = route.steps.slice(checkLarge + 1).includes('QUEUE_ADD_CONFIRMATION');
    if (!hasGuard) return false;
  }

  return true;
}

export function parseActionRoutes(json: string): Record<string, ActionRoute> {
  try {
    const raw: Record<string, StepType[]> = JSON.parse(json);
    return Object.fromEntries(
      Object.entries(raw).map(([name, steps]) => [name, { name, steps }])
    );
  } catch {
    return {};
  }
}
