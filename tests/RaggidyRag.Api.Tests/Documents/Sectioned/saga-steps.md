# Saga steps

A saga moves through steps, one for each message it handles.

## Start

The first event starts the saga and sets its state.

### Correlation

The event carries the id that finds the saga.

## Complete

The saga marks itself complete and its state is removed.
