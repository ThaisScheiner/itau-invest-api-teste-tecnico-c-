#!/bin/sh
# Script para criar tópicos do Kafka de forma robusta

echo "Aguardando 15 segundos para o Kafka estabilizar completamente..."
sleep 15

# Conecta ao broker usando seu nome de serviço 'kafka' e a PORTA INTERNA 29092
echo "Criando tópico 'operacoes-novas' em kafka:29092..."
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic operacoes-novas --partitions 1 --replication-factor 1

echo "Criando tópico 'cotacoes' em kafka:29092..."
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic cotacoes --partitions 1 --replication-factor 1

echo "Criação de tópicos concluída."