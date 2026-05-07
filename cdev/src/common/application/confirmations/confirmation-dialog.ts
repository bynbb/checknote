export interface ConfirmationRequest {
  readonly title: string;
  readonly message: string;
  readonly confirmText: string;
  readonly cancelText: string;
  readonly tone: 'default' | 'danger';
}

export interface ConfirmationDialog {
  confirm(request: ConfirmationRequest): Promise<boolean>;
}
