export interface Query<TResult> {
  readonly type: string;
  readonly __result?: TResult;
}
