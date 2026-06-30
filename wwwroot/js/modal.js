window.showConfirmModal = function (options) {
    const modalElement = document.getElementById("commonConfirmModal");
    const titleElement = document.getElementById("commonConfirmModalLabel");
    const messageElement = document.getElementById("commonConfirmModalMessage");
    const cancelButton = document.getElementById("commonConfirmModalCancel");
    const okButton = document.getElementById("commonConfirmModalOk");

    if (!modalElement || !titleElement || !messageElement || !cancelButton || !okButton) {
        return;
    }

    titleElement.textContent = options.title || "확인";
    messageElement.textContent = options.message || "계속 진행하시겠습니까?";
    cancelButton.textContent = options.cancelText || "취소";
    okButton.textContent = options.okText || "확인";

    const modal = new bootstrap.Modal(modalElement);

    const newOkButton = okButton.cloneNode(true);
    okButton.parentNode.replaceChild(newOkButton, okButton);

    newOkButton.addEventListener("click", function () {
        modal.hide();

        if (typeof options.onConfirm === "function") {
            options.onConfirm();
        }
    });

    modal.show();
};