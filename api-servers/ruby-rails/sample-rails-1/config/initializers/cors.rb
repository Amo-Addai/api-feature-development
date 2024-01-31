Rails.application.config.middleware.insert_before 0, Rack::Cors do
  allow do
    origins '*'
    resource '*',
      headers: :any,
      methods: :any,
      expose: %w[access-token expiry token-type uid client Authorization]
  end
end
