class ICommand {
    execute() { throw new Error("Method execute() must be implemented."); }
    undo() { throw new Error("Method undo() must be implemented."); }
}
