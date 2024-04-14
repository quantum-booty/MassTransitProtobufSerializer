Used https://github.com/MassTransit/MassTransit/pull/4424 as reference.
Note this only works for very simple producer/consumer, and does not work for anything that wraps the Message in another interface, i.e. doesn't work with request/response, job, batch, saga, etc, etc..
