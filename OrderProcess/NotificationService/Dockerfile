# Use the official Python base image
FROM python:3.12.0

# Set the working directory inside the container
WORKDIR /app

# Expose the port on which the application will run
EXPOSE 5000

ENV APP_PORT 5000

# Copy the requirements file to the working directory
COPY requirements.txt .

# Install the Python dependencies
RUN pip install -r requirements.txt

# Copy the application code to the working directory
COPY . .

# Run the FastAPI application using uvicorn server
CMD [ "python3", "-m" , "flask", "run", "--host=0.0.0.0"]
