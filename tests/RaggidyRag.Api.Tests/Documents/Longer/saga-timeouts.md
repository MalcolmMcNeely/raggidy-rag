# Saga timeouts

A saga can ask to be woken after a set time.

## Request

The saga asks for a timeout when it starts to wait.

## Expiry

An expired timeout wakes the saga, and the saga gives up on the reply it waited for.
