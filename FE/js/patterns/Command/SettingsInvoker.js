class SettingsInvoker {
    constructor() {
        this.undoStack = [];
        this.redoStack = [];
    }

    executeCommand(command) {
        command.execute();
        this.undoStack.push(command);
        this.redoStack = []; 
        this.saveToLocal();
    }

    undo() {
        if (this.undoStack.length > 0) {
            const command = this.undoStack.pop();
            command.undo();
            this.redoStack.push(command);
            this.saveToLocal();
            return command.oldSettings;
        }
        return null;
    }

    redo() {
        if (this.redoStack.length > 0) {
            const command = this.redoStack.pop();
            command.execute();
            this.undoStack.push(command);
            this.saveToLocal();
            return command.newSettings;
        }
        return null;
    }

    canUndo() { return this.undoStack.length > 0; }
    canRedo() { return this.redoStack.length > 0; }

    clearHistory() {
        this.undoStack = [];
        this.redoStack = [];
        this.saveToLocal();
    }

    saveToLocal() {
        try {
            // Bọc try-catch để chống Crash khi chạy offline (file:///)
            localStorage.setItem('settings_history', JSON.stringify({
                undos: this.undoStack.length,
                redos: this.redoStack.length
            }));
        } catch (error) {
            console.warn("Cảnh báo: LocalStorage bị chặn (do chạy offline). Chức năng History sẽ lưu trên RAM.");
        }
    }
}
