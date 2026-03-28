class ICommand {
    execute() { throw new Error("Method execute() must be implemented."); }
    undo() { throw new Error("Method undo() must be implemented."); }
}

class ChangeReadingModeCommand extends ICommand {
    constructor(readerContext, newMode, oldMode) {
        super();
        this.readerContext = readerContext;
        this.newMode = newMode;
        this.oldMode = oldMode;
    }
    execute() { this.readerContext.setMode(this.newMode); }
    undo() { this.readerContext.setMode(this.oldMode); }
}

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
            return command.oldMode;
        }
        return null;
    }

    redo() {
        if (this.redoStack.length > 0) {
            const command = this.redoStack.pop();
            command.execute();
            this.undoStack.push(command);
            this.saveToLocal();
            return command.newMode;
        }
        return null;
    }

    canUndo() { return this.undoStack.length > 0; }
    canRedo() { return this.redoStack.length > 0; }

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