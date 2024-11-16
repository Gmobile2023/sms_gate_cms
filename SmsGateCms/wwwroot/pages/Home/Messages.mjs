import { ref } from "vue";
import { useClient, useFormatters } from "@servicestack/vue";
import {GetPartners, GetProviders, QueryMessageTemplates} from "../../mjs/dtos.mjs";
import messageTemplates from "./MessageTemplates.mjs";
// import { CreateMessage, UpdateMessage } from "dtos.mjs"

export default {
    template: /*html*/`
    <auto-query-grid type="Message" :visible-from="{ id:'sm', sms:'lg', status:'md', receiver:'sm', messageTemplateId:'md', partnerId:'lg', providerId:'lg', requestDate:'sm', sentDate:'md', responseDate:'md', telco:'lg', responseMassage:'lg', messageId:'sm' }">
        <template #sms-header>Tin nhắn</template>
        <template #status-header>Trạng thái</template>
        <template #receiver-header>Nguời nhận</template>
        <template #messageTemplate-header>Mẫu tin nhắn</template>
        <template #partner-header>Đối tác</template>
        <template #provider-header>Nhà cung cấp</template>
        <template #requestDate-header>Ngày yêu cầu</template>
        <template #sentDate-header>Ngày gửi</template>
        <template #responseDate-header>Ngày trả về</template>
        <template #telco-header>Công ty viễn thông</template>
        <template #responseMassage-header>Tin nhắn trả về</template>
        
        <template #id="{ id }">
            <span v-html="id"></span>
        </template>

        <template #sms="{ sms }">
            <span v-html="sms"></span>
        </template>

        <template #status="{ status }">
            <span :class="statusClass(status)">{{ statusMap[status] || status }}</span>
        </template>

        <template #receiver="{ receiver }">
            <span v-html="receiver"></span>
        </template>

        <template #messageTemplate= "{ messageTemplate }">
            <text-link v-if="messageTemplate" class="flex items-end" @click.stop="showMessageTemplate(messageTemplateId)" :title="messageTemplate.id">
                <preview-format :value="messageTemplate.content"></preview-format>
            </text-link>
        </template>

         <template #partner="{ partner }">
            <text-link v-if="partner" class="flex items-end" @click.stop="showPartner(partnerId)" :title="partner.id">
                <preview-format :value="partner.partnerName"></preview-format>
            </text-link>
        </template>

        <template #provider="{ provider }">
            <text-link v-if="provider" class="flex items-end" @click.stop="showProvider(providerId)" :title="provider.id">
                <preview-format :value="provider.providerName"></preview-format>
            </text-link>
        </template>

        <template #requestDate="{ requestDate }">
            <span v-html="formatDateTime(requestDate)"></span>
        </template>

        <template #sentDate="{ sentDate }">
            <span v-html="formatDateTime(sentDate)"></span>
        </template>

        <template #responseDate="{ responseDate }">
            <span v-html="formatDateTime(responseDate)"></span>
        </template>

        <template #telco="{ telco }">
            <span v-html="telco"></span>
        </template>

        <template #responseMassage="{ responseMassage }">
            <span v-html="responseMassage"></span>
        </template>

        <template #messageId="{ messageId }">
            <span v-html="messageId"></span>
        </template>
    </auto-query-grid> 
  `,
    setup() {
        const client = useClient();
        const partner = ref();
        const provider = ref();
        const template = ref();
        const formatDateTime = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const timeZoneOffset = 7 * 60 * 60000; // Asia/Ho_Chi_Minh có UTC+7
            const localTime = new Date(date.getTime() + timeZoneOffset);

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };

        const formatDateTimeLocal = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const localTime = new Date(date.getTime());

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };
        async function showPartner(id) {
            const api = await client.api(new GetPartners({ id }))
            if (api.succeeded) {
                partner.value = api.response.results[0]
            }
        }

        async function showProvider(id) {
            const api = await client.api(new GetProviders({ id }))
            if (api.succeeded) {
                provider.value = api.response.results[0]
            }
        }
        
        async function showMessageTemplate(id){
            const api = await client.api(new QueryMessageTemplates({id}))
            if(api.succeeded()){
                template.value = api.response.results[0]
            }
        }
        const statusMap = {
            "Initial": "Khởi tạo",
            "Sent": "Đã gửi",
            "Delivered": "Đã nhận",
            "Failed": "Thất bại"
        };

        const statusClass = (status) => {
            return {
                "text-gray-500": status === "Initial",
                "text-blue-500": status === "Sent",
                "text-green-500": status === "Delivered",
                "text-red-500": status === "Failed"
            };
        };

        const close = () => {
            importFile.value = null;
            partner.value = null;
            provider.value = null;
            template.value = null;
        } 
        return {
            formatDateTime,
            statusMap,
            close,
            formatDateTimeLocal,
            statusClass,
            showPartner,
            showProvider,
            showMessageTemplate,
        }
    }
}