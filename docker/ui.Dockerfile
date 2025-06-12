FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY src/UI/package*.json ./
RUN npm install

# Copy the rest of the code
COPY src/UI/ .

# Build the application
RUN npm run build

# Production image
FROM node:18-alpine
WORKDIR /app

# Copy built assets
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/package*.json ./
COPY --from=build /app/node_modules ./node_modules

# Expose port
EXPOSE 3000

# Start the application
CMD ["npm", "start"] 